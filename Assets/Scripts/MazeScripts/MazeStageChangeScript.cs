using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MazeStageChangeScript : MonoBehaviour
{
    public TextMeshProUGUI stageTitleText;
    public SqueezeToContinueScript squeezeScript;
    public GameObject mapContGO;
    public GameObject extraInfoIcon;
    public Sprite icon;
    public string iconText;
    private IconTextScript iconScript;
    private CanvasGroup group;
    private MazeNewGameController mapCont;
    private const float DELTA_ALPHA = 0.02f;
    private float currentAlpha = 0f;
    private bool deactivateTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        iconScript = extraInfoIcon.GetComponent<IconTextScript>();
        iconScript.Setup(icon, iconText);
        mapCont = mapContGO.GetComponent<MazeNewGameController>();
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

    public void SetUIText(string bigTxt, bool showIcon)
    {
        stageTitleText.text = bigTxt;
        if (showIcon)
        {
            extraInfoIcon.SetActive(true);
        } else {
            extraInfoIcon.SetActive(false);
        }
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
            TriggerDeactivation();
        }
    }

    public void TriggerDeactivation()
    {
        mapCont.GenerateStageMap();
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
