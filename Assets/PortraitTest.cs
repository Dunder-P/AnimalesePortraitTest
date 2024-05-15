using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
public class PortraitTest : MonoBehaviour
{
    /// <summary>
    /// The animalese object
    /// </summary>
    [SerializeField] private Animalese myAnimalese;
    /// <summary>
    /// The voicebank selection dropdown
    /// </summary>
    [SerializeField] private TMP_Dropdown vbDropdown;
    /// <summary>
    /// The text the user can input to be spoken
    /// </summary>
    [SerializeField] private TextMeshProUGUI inputText;
    /// <summary>
    /// Image object displaying the NPC's portrait
    /// </summary>
    [SerializeField] private Image npcPortrait;
    /// <summary>
    /// Open closed and mid-speech sprites
    /// </summary>
    [SerializeField] private Sprite talk_open, talk_closed, talk_mid;
    [SerializeField] private TextMeshProUGUI opPath, clPath, midPath;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Awake()
    {
        
        List<string> av = new List<string>();
        foreach(AnimaleseVoicebank x in myAnimalese.animals)
        {
            av.Add(x.voicebankName);
        }
        vbDropdown.ClearOptions();
        vbDropdown.AddOptions(av);
    }
    public void speakText()
    {
        myAnimalese.Speak(inputText.text);
        StartCoroutine((lipFlap()));
    }
    /// <summary>
    /// updates the portraits based on given paths
    /// </summary>
    public void updatePortraits()
    {
        Sprite tempSprite;
        //try
        //{
            tempSprite = setUpSprite(opPath.text);
            talk_open = tempSprite;
        //}
        //catch
        //{
            opPath.text = "INVALID PATH";
        //}
        //try
        //{
            tempSprite = setUpSprite(clPath.text);
            talk_closed = tempSprite;
        //}
        //catch
        //{
            clPath.text = "INVALID PATH";
        //}
        //try
        //{
            tempSprite = setUpSprite(midPath.text);
            talk_mid = tempSprite;
        //}
        //catch
        //{
            midPath.text = "INVALID PATH";
        //}
        npcPortrait.sprite = talk_closed;
  

    }
    public Sprite setUpSprite(string filepath)
    {
        filepath = filepath.Replace("\"", "");
        filepath = filepath.Replace("\u200B", "");
        filepath = Path.GetFullPath((filepath));
        byte[] rawPortrait = File.ReadAllBytes(filepath);
        Texture2D portraitTexture = new Texture2D(500, 500);
        portraitTexture.LoadImage(rawPortrait);
        portraitTexture.Reinitialize(portraitTexture.width, portraitTexture.height);
        portraitTexture.LoadImage(rawPortrait);
        return Sprite.Create(portraitTexture, new Rect(0, 0, portraitTexture.width, portraitTexture.height), new Vector2(0, 0));
    }
    /// <summary>
    /// Swaps the animalese voicebank to the one with the index of dropdown.
    /// </summary>
    public void swapVoicebank(int index)
    {
        myAnimalese.UpdateVoicebank(index);
    }
    public IEnumerator lipFlap()
    {
        AudioSource animalSource = myAnimalese.channels[0];
        float updateStep = 0.1f;
        int sampleDataLength = 1024;

        float currentUpdateTime = 0f;

        float clipLoudness;
        float[] clipSampleData = new float[sampleDataLength];
        float[] samples = new float[animalSource.clip.samples * animalSource.clip.channels];
        animalSource.clip.GetData(samples, 0);
        float averageVolume = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            averageVolume += Mathf.Abs(samples[i]);
        }
        averageVolume /= samples.Length;
        while (animalSource.isPlaying)
        {
            currentUpdateTime += Time.deltaTime;
            if (currentUpdateTime >= updateStep)
            {
                currentUpdateTime = 0f;
                animalSource.clip.GetData(clipSampleData, animalSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                clipLoudness = 0f;
                foreach (var sample in clipSampleData)
                {
                    clipLoudness += Mathf.Abs(sample);
                }
                clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for
                                                  //print(clipLoudness);
                if (clipLoudness > .1f)
                {
                    npcPortrait.sprite = talk_open;
                }
                else if (clipLoudness > .01f)
                {
                    npcPortrait.sprite = talk_mid;
                }
                else
                {
                    npcPortrait.sprite = talk_closed;
                }
                yield return new WaitForSeconds(.01f);
            }
        }
        npcPortrait.sprite = talk_closed;
    }
}
