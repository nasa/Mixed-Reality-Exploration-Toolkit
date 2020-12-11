using UnityEngine;
using UnityEngine.UI;

public class LoadingIndicatorManager : MonoBehaviour
{
    public Animator loadingAnimator;
    public Text loadingText;

    public void ShowLoadingIndicator(string loadingMessage)
    {
        loadingText.text = loadingMessage;
        loadingAnimator.gameObject.SetActive(true);
        loadingText.gameObject.SetActive(true);
    }

    public void StopLoadingIndicator()
    {
        loadingAnimator.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
    }
}