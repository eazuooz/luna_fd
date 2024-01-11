using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LunaAssetBundle : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject NormalHit;
    public GameObject NormalDeath;
    public GameObject TowerDeath;
    public GameObject AllyAttacked;

    public GameObject Coin;

    public GameObject HealthBarAlly;
    public GameObject HealthBarEnemy;
    public Text DamageText;

    public GameObject pochiBarrierEffect;
    public GameObject BleedEffect;
    public GameObject BerserkEffect;
    public GameObject StunEffect;
    public GameObject JumpEffect;
    public GameObject beleifWaveEffect;
    public GameObject serenBlessing;
    public GameObject loneBlessing;

    public List<GameObject> BuffIcons = new List<GameObject>();

    public GameObject MiniPlayerEffectForUnit;

    public AudioClip sound_slash;
    public AudioClip sound_hit;
    public AudioClip sound_bleed;

    public List<AudioClip> ArrowSounds = new List<AudioClip>();
    public List<AudioClip> ThrowSounds = new List<AudioClip>();
    public List<AudioClip> SwordSounds = new List<AudioClip>();

    public AudioClip RandomArrowSound()
    {
        if (ArrowSounds.Count != 0)
        {
            return ArrowSounds[Random.Range(0, ArrowSounds.Count)];
        }
        else
            return null;
    }

    public AudioClip RandomThrowSound()
    {
        if (ThrowSounds.Count != 0)
        {
            return ThrowSounds[Random.Range(0, ThrowSounds.Count)];
        }
        else
            return null;
    }

    public AudioClip RandomSwordSound()
    {
        if (SwordSounds.Count != 0)
        {
            return SwordSounds[Random.Range(0, SwordSounds.Count)];
        }
        else
            return null;
    }
}
