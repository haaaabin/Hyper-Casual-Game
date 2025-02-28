using System.Collections;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class StickManManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem blood;
    private Rigidbody rigid;
    private CapsuleCollider coll;
    private Animator anim;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();

        GameManager.setMenuDelegate += OnMenuAnimState;
        GameManager.setGameDelegate += OnGameAnimState;
    }

    void OnDestroy()
    {
        GameManager.setMenuDelegate -= OnMenuAnimState;
        GameManager.setGameDelegate -= OnGameAnimState;
    }

    private void OnMenuAnimState()
    {
        anim.SetBool("run", false);
    }
    private void OnGameAnimState()
    {
        anim.SetBool("run", true);
    }

    void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "red":
                Instantiate(blood, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
                break;

            case "ramp":

                if (transform != null && transform.gameObject.activeSelf)
                    transform.DOJump(transform.position, 3f, 1, 1).SetEase(Ease.Flash).OnComplete(() => PlayerManager.instance.StartCoroutine(DelayedFormatStickMan(1.3f)));
                break;

            case "stair":
                transform.parent.parent = null;
                transform.parent = null;
                coll.isTrigger = false;
                rigid.isKinematic = false;

                anim.SetBool("run", false);

                {
                    StairSoundManager.instance.PlayNote(stair.GetStairIndex());
                }

                if (!PlayerManager.instance.moveTheCamera)
                    PlayerManager.instance.moveTheCamera = true;

                if (PlayerManager.instance.transform.childCount == 0)
                {
                    StartCoroutine(ChangeStairRender(other));
                    StartCoroutine(UIManager.instance.UpdateScore(PlayerManager.instance.numberOfStickmans, UpdateTextWithScore(other.gameObject)));
                }

                break;

            case "Prop":
                if (PlayerManager.instance.numberOfStickmans > 0)
                {
                    ObjectPool.instance.ReturnPlayerObject(transform.gameObject); // 객체 풀로 반환
                    Instantiate(blood, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);

                    PlayerManager.instance.numberOfStickmans--;
                    PlayerManager.instance.counterTxt.text = PlayerManager.instance.numberOfStickmans.ToString();
                }
                break;
        }
    }

    IEnumerator ChangeStairRender(Collider other)
    {
        yield return new WaitForSeconds(0.5f);

        Renderer stairRender = other.GetComponent<Renderer>();
        if (stairRender != null)
        {
            stairRender.material.DOColor(new Color(0.4f, 0.98f, 0.65f), 0.5f).SetLoops(1000, LoopType.Yoyo)
                .SetEase(Ease.Flash);
        }
        yield return new WaitForSeconds(0.01f);
        GameManager.Instance().ChangeState(GameManager.GameState.LEVELCOMPLETE);

    }

    IEnumerator DelayedFormatStickMan(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayerManager.instance.FormatStickMan();
    }

    private float UpdateTextWithScore(GameObject gameObject)
    {
        string scoreText = Regex.Match(gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text, @"\d+(\.\d+)?").Value;

        if (float.TryParse(scoreText, out float number))
        {
            return number;
        }
        else
        {
            return 0f;
        }
    }
}