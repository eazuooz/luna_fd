using UnityEngine;
using UnityEngine.UI;

/// <summary>Cooldown label binding; the owning UI form supplies its current value.</summary>
public sealed class PlayerSkillCoolChecker : MonoBehaviour
{
    private Text label;
    private int remaining;
    public int skillCooltime
    {
        get => remaining;
        set
        {
            remaining = Mathf.Max(0, value);
            if (label == null)
            {
                Transform child = transform.Find("SkillCool");
                if (child != null) label = child.GetComponent<Text>();
            }
            if (label != null) label.text = remaining == 0 ? string.Empty : remaining.ToString();
        }
    }
}
