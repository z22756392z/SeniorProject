using UnityEngine;
using UnityEngine.UI;

public class SwiptMenu : MonoBehaviour
{
    public GameObject scrollbar;
    float scroll_pos = 0, distance;
    float[] pos;

    void Update()
    {
        pos = new float[transform.childCount];
        distance = 1f / (pos.Length - 1f);
        scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
        if (Input.GetMouseButton(0))
        {
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
            SetObjDistance(pos, scroll_pos, distance);
        }
        else
        {
            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                {
                    scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                    SetObjDistance(pos, scroll_pos, distance);
                }
            }
        }
    }

    void SetObjDistance(float[] pos, float scroll_pos, float distance) // 讓他有距離感
    {
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1.3f, 1.3f), 0.1f);  // 選到的物件變大
                for (int a = 0; a < pos.Length; a++)
                {
                    if (a != i)
                    {
                        transform.GetChild(a).localScale = Vector2.Lerp(transform.GetChild(a).localScale, new Vector2(.8f, .8f), 0.1f);
                    }
                }
            }
        }
    }
}
