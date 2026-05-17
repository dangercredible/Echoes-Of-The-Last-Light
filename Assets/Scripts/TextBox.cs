using UnityEngine;

public class TextBox : MonoBehaviour
{
    public GameObject textBoxx;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textBoxx.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            textBoxx.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            textBoxx.SetActive(false);
        }
    }
}
