using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WhenPigsFlyStageChangeScript : MonoBehaviour
{
    public TextMeshProUGUI stageTitleText;
    public GameObject pigsGameContGO;
    private WhenPigsFlyGameController pigsGameContScript;
    private CanvasGroup group;
    private const float DELTA_ALPHA = 0.02f;
    private float currentAlpha = 0f;
    private bool deactivateTriggered = false;
    public SqueezeToContinueScript squeezeScript;

    // Start is called before the first frame update
    void Start()
    {
        pigsGameContScript = pigsGameContGO.GetComponent<WhenPigsFlyGameController>();
        group = GetComponent<CanvasGroup>();
        group.alpha = 0;
        group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (deactivateTriggered)
        {
            ProgressFadeOut();
        }
        else
        {
            BlockUntilExitTrigger();
        }
    }

    public void SetUIText(string bigTxt)
    {
        stageTitleText.text = bigTxt;
    }

    public void TriggerStageChange()
    {
        Time.timeScale = 0;
        deactivateTriggered = false;
        group.blocksRaycasts = true;
        gameObject.SetActive(true);
        squeezeScript.Activate();
    }

    private void BlockUntilExitTrigger()
    {
        if (currentAlpha < 1)
        {
            currentAlpha += DELTA_ALPHA;
            if (currentAlpha > 1)
                currentAlpha = 1;

            group.alpha = currentAlpha;
        }

        if (Input.GetKey("enter") || Input.GetKey("return"))
        {
            GoToNextStage();
        }
    }

    public void GoToNextStage()
    {
        pigsGameContScript.SetStage();
        deactivateTriggered = true;
    }

    private void ProgressFadeOut()
    {
        currentAlpha -= DELTA_ALPHA;
        if (currentAlpha < 0f)
        {
            currentAlpha = 0f;
            Time.timeScale = 1;
            group.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        group.alpha = currentAlpha;
    }
}
