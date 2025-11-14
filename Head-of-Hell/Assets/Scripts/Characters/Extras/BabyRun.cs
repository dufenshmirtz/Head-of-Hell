using UnityEngine;

public class BabyRun : MonoBehaviour
{
    public float speed = 5f;
    private int direction = 1;

    public void SetDirection(int dir)
    {
        direction = dir;
    }

    void Update()
    {
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
    }
}
