using DG.Tweening;

using UnityEngine;

public class MenuScript : MonoBehaviour
{
    public CanvasGroup mainMenu, controlsMenu;
    public float transitionTime = 1f;

    public void StartGame()
    {
        // Functionality to start the game goes here
        StartCoroutine(GameManager.Instance.LoadGame());
    }

    public void MenuControls(bool enter)
    {
        if(enter)
        {
            DOTween.Sequence().Append(controlsMenu.DOFade(1f, transitionTime).OnStart(() => controlsMenu.blocksRaycasts = true))
                              .Join(mainMenu.DOFade(0f, transitionTime).OnStart(() => mainMenu.blocksRaycasts = false));
        }
        else
        {
            DOTween.Sequence().Append(mainMenu.DOFade(1f, transitionTime).OnStart(() => mainMenu.blocksRaycasts = true))
                              .Join(controlsMenu.DOFade(0f, transitionTime).OnStart(() => controlsMenu.blocksRaycasts = false));
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
