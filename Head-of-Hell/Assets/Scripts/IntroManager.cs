using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Wait : MonoBehaviour
{
    public float wait_time = 5.4f;

    void Start()
    {
        StartCoroutine(Wait_for_Seconds());
    }

    IEnumerator Wait_for_Seconds()
    {
        yield return new WaitForSeconds(wait_time);
        SceneManager.LoadScene(0); //menu scene 
    }
}
