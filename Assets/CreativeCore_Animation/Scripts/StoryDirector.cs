using System.Collections;
using UnityEngine;

// 挂在主角 (Karter) 身上。按顺序触发 Animator 里的 Trigger 来讲故事。
// 自动模式：开始后按时间表依次播放；也可以按 1 / 2 / 3 手动触发。
public class StoryDirector : MonoBehaviour
{
    [Header("Trigger 名字必须和 Animator 参数完全一致")]
    public string walkTrigger = "Walk";
    public string cheerTrigger = "Cheer";
    public string danceTrigger = "Dance";

    [Header("自动播放时间（秒）")]
    public bool autoPlay = true;
    public float walkAt = 3f;      // 开场站立几秒后开始走
    public float cheerAt = 6f;     // 走到水果摊，发现水果在动
    public float danceAt = 9f;     // 一起跳舞

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (autoPlay) StartCoroutine(PlayStory());
    }

    IEnumerator PlayStory()
    {
        yield return new WaitForSeconds(walkAt);
        anim.SetTrigger(walkTrigger);
        yield return new WaitForSeconds(cheerAt - walkAt);
        anim.SetTrigger(cheerTrigger);
        yield return new WaitForSeconds(danceAt - cheerAt);
        anim.SetTrigger(danceTrigger);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) anim.SetTrigger(walkTrigger);
        if (Input.GetKeyDown(KeyCode.Alpha2)) anim.SetTrigger(cheerTrigger);
        if (Input.GetKeyDown(KeyCode.Alpha3)) anim.SetTrigger(danceTrigger);
    }
}
