using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class S06 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(RotateNshoot(90,5));
        }
    }
    public IEnumerator TestCoroutine(int count)
    {
        while (count > 0)
        {
            Debug.Log(count);
            count--;
            yield return null;
        }
        
        Debug.Log("Coroutine finished");
    }
    public IEnumerator RotateNshoot(float angle , float duration)
    {
        
        Transform startRotation = transform;
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, angle, 0));
        Vector3 targetPos = transform.position + transform.forward * 5;

        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
        timeElapsed = 0;
        while (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(startRotation.position, targetPos, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }




        Debug.Log("mmm");

        

        Debug.Log("Coroutine finished");
        yield break;
    }
}
