using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("NGUI/Tween/Number")]
public class TweenNumber : UITweener
{
    /// <summary>
    /// duongrs: change tween float number to int value;
    /// </summary>
    public long @from = 0;

    public long to = 1;
    public string header = string.Empty;
    private Transform mTrans;
    private TextMeshProUGUI mLabel;
    private Canvas mPanel;

    //public bool formatNumber = false;

    /// <summary>
    /// Current alpha.
    /// </summary>
    /// /// <summary>
    /// duongrs: change tween int number to long value;
    /// </summary>
    private long _number;

    public long number
    {
        get
        {
            return _number;
        }
        set
        {
            _number = value;
            if (mLabel)
            {
                mLabel.text = header + _number;

                //mLabel.text = formatNumber ? (header + _number) : (header + Utils.FormatResource(_number));
            }
        }
    }

    /// <summary>
    /// Find all needed components.
    /// </summary>

    private void Awake()
    {
        mPanel = GetComponent<Canvas>();
        if (mPanel == null) mLabel = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// Interpolate and update the alpha.
    /// </summary>

    override protected void OnUpdate(float factor, bool isFinished)
    {
        number = (int)Mathf.Lerp(@from, to, factor);
    }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenNumber Begin(GameObject go, float duration, long number)
    {
        TweenNumber comp = UITweener.Begin<TweenNumber>(go, duration);
        comp.from = comp.number;
        comp.to = number;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }
}