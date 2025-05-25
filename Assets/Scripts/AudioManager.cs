using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private APIManager API;
    [SerializeField] private AudioSource BGM;
    [SerializeField] private AudioSource SFX;
    [SerializeField] private float bgmVol = 0.5f;

    public bool enableBGM = true;
    public bool enableSFX = true;

    public AudioClip[] bgmClip;
    public AudioClip[] sfxClip;

    private void Awake()
    {
        BGM.ignoreListenerVolume = true;
        BGM.loop = true;
        BGM.volume = bgmVol;
    }

    public void PlayBGM(int trackIndex = 0)
    {
        BGM.clip = bgmClip[trackIndex];
        BGM.Play();
    }

    public void PlaySFX(int trackIndex = 0)
    {
        SFX.PlayOneShot(sfxClip[trackIndex]);
    }

    public void StopSFX()
    {
        SFX.Stop();
    }

    public void ToogleBgm()
    {
        enableBGM = !enableBGM;
        EnableBGM();
    }

    void EnableBGM()
    {
        BGM.volume = enableBGM ? bgmVol : 0;
    }

    public void ToogleSfx()
    {
        enableSFX = !enableSFX;
        EnableSFX();
    }

    void EnableSFX()
    {
        AudioListener.volume = enableSFX ? 1 : 0;
    }

    public void SaveAndSetAudioSetting(Sounds soundData, GameObject bgmToogle, GameObject sfxToogle)
    {
        enableBGM = soundData.music;
        enableSFX = soundData.effect;
        EnableBGM();
        EnableSFX();
        SetAudioSetting(bgmToogle, sfxToogle);
        if (enableBGM && !BGM.isPlaying)
        {
            PlayBGM();
        }
    }

    public void SetAudioSetting(GameObject bgmToogle, GameObject sfxToogle, bool isSaving = false)
    {
        bgmToogle.transform.GetChild(0).gameObject.SetActive(!enableBGM);
        bgmToogle.transform.GetChild(1).gameObject.SetActive(enableBGM);
        sfxToogle.transform.GetChild(0).gameObject.SetActive(!enableSFX);
        sfxToogle.transform.GetChild(1).gameObject.SetActive(enableSFX);
        if (isSaving)
        {
            Sounds soundSetting = new()
            {
                music = enableBGM,
                effect = enableSFX
            };
            StartCoroutine(API.SendSoundSettingIE(soundSetting));
        }
    }
}
