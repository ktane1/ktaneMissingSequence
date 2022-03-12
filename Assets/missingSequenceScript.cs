using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class missingSequenceScript : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
	public KMBombModule module;

    public KMSelectable screen;
    public KMSelectable[] keypadButtons;
    public GameObject[] rectangles;
    public GameObject[] mainRectangles;
    public GameObject[] mainRectangleHighlight;
    public TextMesh[] rectangleText;
    public TextMesh[] mainRectangleText;
    public TextMesh countdownText;
    public Material[] colours;
    public Transform mask;
    public GameObject keypad;

    private float[] scrollSpeed = new float[6];
    private bool screenActivated;

    private int[] selectedColours = new int[6];
    private int[][] generatedSequences = new int[6][] { new int[6], new int[6], new int[6], new int[6], new int[6], new int[6] };
    private int[] sequenceAnswers = new int[6];

    private int selectedRectangle = 0;
    private int[] mainRectangleInputs = new int[6];
    private int[] missingSequences = new int[6] { 0, 1, 2, 3, 4, 5 };
    private bool[] answerChecks = new bool[6];


    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool isAnimating, inputMode, striked, moduleSolved; // Some helpful booleans

    void Awake()
    {
    	moduleId = moduleIdCounter++;
        screen.OnInteract += delegate
        {
            screenHandler();
            return false;
        };
        for (int i = 0; i < keypadButtons.Length; i++)
        {
            int j = i;
            keypadButtons[j].OnInteract += () => { keypadHandler(j); return false; };
        }
    }

    void Start()
    {
        int[] coloursDup = new int[12] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        for (int i = 0; i < selectedColours.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(0, coloursDup.Length);
            selectedColours[i] = coloursDup[rnd];
            coloursDup = coloursDup.Where(val => val != selectedColours[i]).ToArray();
        }
        for (int i = 0; i < rectangles.Length; i++)
        {
            rectangles[i].SetActive(false);
            rectangles[i].GetComponent<MeshRenderer>().material = colours[selectedColours[i % 6]];
        }
        for (int i = 0; i < mainRectangles.Length; i++)
        {
            mainRectangles[i].SetActive(false);
            mainRectangles[i].GetComponent<MeshRenderer>().material = colours[selectedColours[i]];
            mainRectangleInputs[i] = 0;
            mainRectangleText[i].text = mainRectangleInputs[i].ToString();
        }
        sequenceGenerator();
    }

    void screenHandler()
    {
        if (moduleSolved || isAnimating) { return; }
        if (!screenActivated)
        {
            screenActivated = true;
            StartCoroutine(countdown());
        }
        else if (!inputMode)
        {
            inputMode = true;
            audio.PlaySoundAtTransform("transition", transform);
            StartCoroutine(maskChange());
            StartCoroutine(keypadReveal());
            StartCoroutine(startInputMode());
        }
        else
        {
            Debug.LogFormat("[Missing Sequence #{0}]: The values are submitted! Let's see...", moduleId);
            audio.PlaySoundAtTransform("transition", transform);
            inputMode = false;
            for (int i = 0; i < answerChecks.Length; i++)
            {
                string chosenColour = colours[selectedColours[i]].name;
                chosenColour = chosenColour.Replace("Mat", "");
                if (mainRectangleInputs[i] == sequenceAnswers[i])
                {
                    answerChecks[i] = true;
                    Debug.LogFormat("[Missing Sequence #{0}]: Input {1} matches with answer {2} for the {3} sequence...", moduleId, mainRectangleInputs[i].ToString(), sequenceAnswers[i].ToString(), chosenColour);
                }
                else
                {
                    answerChecks[i] = false;
                    Debug.LogFormat("[Missing Sequence #{0}]: Input {1} doesn't match with answer {2} for the {3} sequence...", moduleId, mainRectangleInputs[i].ToString(), sequenceAnswers[i].ToString(), chosenColour);
                }
            }
            if (answerChecks.All(x => x))
            {
                Debug.LogFormat("[Missing Sequence #{0}]: All inputs are correct, module solved!", moduleId);
                audio.PlaySoundAtTransform("transition", transform);
                audio.PlaySoundAtTransform("solve", transform);
                module.HandlePass();
                moduleSolved = true;
            }
            else
            {
                Debug.LogFormat("[Missing Sequence #{0}]: Some of the inputs are wrong, strike!", moduleId);
                audio.PlaySoundAtTransform("strike", transform);
                module.HandleStrike();
                striked = true;
                for (int i = 0; i < answerChecks.Length; i++)
                {
                    if (!answerChecks[i])
                    {
                        mainRectangleText[i].color = new Color(1f, 0f, 0f, 1f);
                    }
                    else
                    {
                        mainRectangleText[i].color = new Color(0f, 1f, 0f, 1f);
                        rectangleText[missingSequences[i] * 6 + i].text = mainRectangleInputs[i].ToString();
                    }
                }
            }
            StartCoroutine(maskChange());
            StartCoroutine(keypadReveal());
        }
    }

    void keypadHandler(int k)
    {
        if (moduleSolved || isAnimating || !inputMode) { return; }
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (k < 10)
        {
            mainRectangleInputs[selectedRectangle] = Convert.ToInt32(mainRectangleInputs[selectedRectangle].ToString() + k.ToString()) % 10000;
            mainRectangleText[selectedRectangle].text = mainRectangleInputs[selectedRectangle].ToString();
        }
        if (k == 10)
        {
            mainRectangleInputs[selectedRectangle] = -1 * mainRectangleInputs[selectedRectangle];
            mainRectangleText[selectedRectangle].text = mainRectangleInputs[selectedRectangle].ToString();
        }
        if (k == 11)
        {
            selectedRectangle++;
            if (selectedRectangle > 5) { selectedRectangle = 0; }
            while (answerChecks[selectedRectangle])
            {
                selectedRectangle++;
                if (selectedRectangle > 5) { selectedRectangle = 0; }
            }
            for (int j = 0; j < mainRectangleHighlight.Length; j++)
            {
                if (j == selectedRectangle && !answerChecks[j])
                {
                    mainRectangleHighlight[j].SetActive(true);
                }
                else
                {
                    mainRectangleHighlight[j].SetActive(false);
                }
            }
            mainRectangleText[selectedRectangle].text = mainRectangleInputs[selectedRectangle].ToString();
        }
    }

    void sequenceGenerator()
    {
        StringBuilder sb = new StringBuilder();
        string pattern = "";//Logging
        missingSequences.Shuffle();
        for (int i = 0; i < generatedSequences.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(0, 5);
            int start = 0;
            int secondStart = 0;
            int offStart = 0;
            int offset = 0;
            int k = 0;
            List<int> sequence = new List<int>();
            switch (rnd)
            {
                case 0:
                    k = UnityEngine.Random.Range(0, 2);
                    if (k == 0)
                    {
                        start = UnityEngine.Random.Range(-1000, 1000);
                        offStart = UnityEngine.Random.Range(-100, 100);
                        offset = UnityEngine.Random.Range(-100, 100);
                        sequence = sequencePatterns.APAPOff(start, offStart, offset, 6);
                        sb.Append("an arithmetic progression among the offsets with offset " + offset);
                        break;
                    }
                    else
                    {
                        start = UnityEngine.Random.Range(-100, 100);
                        k = UnityEngine.Random.Range(0, 2);
                        if (k == 0)
                        {
                            offStart = UnityEngine.Random.Range(-4, 0);
                        }
                        else
                        {
                            offStart = UnityEngine.Random.Range(1, 5);
                        }
                        k = UnityEngine.Random.Range(0, 2);
                        if (k == 0)
                        {
                            offset = UnityEngine.Random.Range(-5, -1);
                        }
                        else
                        {
                            offset = UnityEngine.Random.Range(2, 6);
                        }
                        sequence = sequencePatterns.GPAPOff(start, offStart, offset, 6);
                        sb.Append("a geometric progression among the offsets with offset " + offset);
                        break;
                    }

                case 1:
                    k = UnityEngine.Random.Range(0, 4);
                    switch (k)
                    {
                        case 0:
                            start = UnityEngine.Random.Range(-100, 100);
                            secondStart = UnityEngine.Random.Range(start + 1, start + 100);
                            sequence = sequencePatterns.Fib(start, secondStart, 6);
                            sb.Append("the Fibonacci progression");
                            break;
                        case 1:
                            start = UnityEngine.Random.Range(-100, 100);
                            secondStart = UnityEngine.Random.Range(start + 1, start + 100);
                            sequence = sequencePatterns.RecursiveEz(start, secondStart, 6);
                            sb.Append("a recursive function of a-b, where a is the second previous term and b is the previous term");
                            break;
                        case 2:
                            start = UnityEngine.Random.Range(-20, 20);
                            secondStart = UnityEngine.Random.Range(start + 1, start + 50);
                            offset = UnityEngine.Random.Range(2, 6);
                            sequence = sequencePatterns.RecursiveMed(start, secondStart, offset, 6);
                            sb.Append("a recursive function of " + offset + "a+b, where a is the second previous term and b is the previous term");
                            break;
                        case 3:
                            start = UnityEngine.Random.Range(-3, 3);
                            secondStart = UnityEngine.Random.Range(start + 1, start + 3);
                            sequence = sequencePatterns.RecursiveProd(start, secondStart, 6);
                            sb.Append("a recursive function of a+b-ab, where a is the second previous term and b is the previous term");
                            break;
                    }
                    break;
                case 2:
                    start = UnityEngine.Random.Range(0, 5000);
                    k = UnityEngine.Random.Range(0, 8);
                    switch (k)
                    {
                        case 0:
                            sb.Append("the digital root as offset");
                            break;
                        case 1:
                            sb.Append("the sum of digits as offset");
                            break;
                        case 2:
                            while (start.ToString().Contains("0") || (start + sequencePatterns.ProdOfDigits(start)).ToString().Contains("0") || (start - sequencePatterns.ProdOfDigits(start)).ToString().Contains("0"))
                            {
                                start++;
                            }
                            sb.Append("the product of digits as offset");
                            break;
                        case 3:
                            start = UnityEngine.Random.Range(-1000, 1000);
                            sb.Append("the prime offsets");
                            break;
                        case 4:
                            start = UnityEngine.Random.Range(-1000, 1000);
                            sb.Append("the perfect square offsets");
                            break;
                        case 5:
                            start = UnityEngine.Random.Range(-250, 250);
                            sb.Append("the perfect cube offsets");
                            break;
                        case 6:
                            start = sequencePatterns.primes[UnityEngine.Random.Range(1, 10)] * sequencePatterns.primes[UnityEngine.Random.Range(1, 10)];
                            sb.Append("the largest prime factor as offset");
                            break;
                        case 7:
                            start = sequencePatterns.primes[UnityEngine.Random.Range(1, 10)] * sequencePatterns.primes[UnityEngine.Random.Range(1, 10)];
                            sb.Append("the sum of all prime factors as offset");
                            break;
                        default:
                            break;
                    }
                    sequence = sequencePatterns.Special(start, k, 6);
                    break;

                case 3:
                    k = UnityEngine.Random.Range(0, 3);
                    switch (k)
                    {
                        case 0:
                            k = UnityEngine.Random.Range(0, 2);
                            if (k == 0) { start = UnityEngine.Random.Range(1, 8); }
                            else { start = UnityEngine.Random.Range(-8, 0); }
                            k = UnityEngine.Random.Range(0, 2);
                            if (k == 0) { secondStart = UnityEngine.Random.Range(1, 3); }
                            else { secondStart = UnityEngine.Random.Range(-3, 0); }
                            k = UnityEngine.Random.Range(0, 2);
                            if (k == 0) { offStart = UnityEngine.Random.Range(1, 5); }
                            else { offStart = UnityEngine.Random.Range(-4, -1); }
                            k = UnityEngine.Random.Range(0, 2);
                            if (k == 0) { offset = UnityEngine.Random.Range(2, 3); }
                            else { offset = UnityEngine.Random.Range(-2, 0); }
                            sequence = sequencePatterns.Comb(sequencePatterns.AP(start, offStart, 6), sequencePatterns.GP(secondStart, offset, 6), 6);
                            sb.Append("a combination via multiplication of an arithmetic progression with first term " + start + " and offset " + offStart + " and a geometric progression with first term " + secondStart + " and offset " + offset);
                            break;
                        case 1:
                            k = UnityEngine.Random.Range(0, 2);
                            if (k == 0) { start = UnityEngine.Random.Range(1, 10); }
                            else { start = UnityEngine.Random.Range(-10, 0); }
                            k = UnityEngine.Random.Range(0, 2);
                            if (k == 0) { secondStart = UnityEngine.Random.Range(1, 10); }
                            else { secondStart = UnityEngine.Random.Range(-10, 0); }
                            k = UnityEngine.Random.Range(0, 2);
                            if (k == 0) { offStart = UnityEngine.Random.Range(1, 10); }
                            else { offStart = UnityEngine.Random.Range(-10, 0); }
                            k = UnityEngine.Random.Range(0, 2);
                            if (k == 0) { offset = UnityEngine.Random.Range(1, 10); }
                            else { offset = UnityEngine.Random.Range(-10, 0); }
                            sequence = sequencePatterns.Comb(sequencePatterns.AP(start, offStart, 6), sequencePatterns.AP(secondStart, offset, 6), 6);
                            sb.Append("a combination via multiplication of two arithmetic progressions, one with first term " + start + " and offset " + offStart + " and another with first term " + secondStart + " and offset " + offset);
                            break;
                        case 2:
                            k = UnityEngine.Random.Range(0, 19);
                            List<int> primes = new List<int>();
                            for (int j = 0; j < 6; j++)
                            {
                                primes.Add(sequencePatterns.primes[k + j]);
                            }
                            start = UnityEngine.Random.Range(-50, 50);
                            k = UnityEngine.Random.Range(0, 2);
                            if (k == 0) { offset = UnityEngine.Random.Range(1, 10); }
                            else { offset = UnityEngine.Random.Range(-10, -1); }
                            sequence = sequencePatterns.Comb(sequencePatterns.AP(start, offset, 6), primes, 6);
                            sb.Append("a combination via multiplication of an arithmetic progression with first term " + start + " and offset " + offset + ", and a set of ascending prime numbers starting with " + primes[0]);
                            break;
                    }
                    break;

                case 4:
                    k = UnityEngine.Random.Range(0, 3);
                    offset = UnityEngine.Random.Range(-5, 6);
                    switch (k)
                    {
                        case 0:
                            start = UnityEngine.Random.Range(0, sequencePatterns.primes.Count - 6);
                            sb.Append("a set of primes with offset " + offset + " starting from " + sequencePatterns.primes[start]);
                            break;
                        case 1:
                            start = UnityEngine.Random.Range(0, sequencePatterns.squares.Count - 6);
                            sb.Append("a set of perfect squares with offset " + offset + " starting from " + sequencePatterns.squares[start]);
                            break;
                        case 2:
                            start = UnityEngine.Random.Range(0, sequencePatterns.cubes.Count - 6);
                            sb.Append("a set of perfect cubes with offset " + offset + " starting from " + sequencePatterns.cubes[start]);
                            break;
                    }
                    sequence = sequencePatterns.SpecialTerms(start, k, offset, 6);
                    break;
            }
            pattern = sb.ToString();
            sb.Remove(0, sb.Length);

            for (int j = 0; j < generatedSequences[i].Length; j++)
            {
                generatedSequences[i][j] = sequence[j];
                if (j == missingSequences[i])
                {
                    sequenceAnswers[i] = generatedSequences[i][j];
                    rectangleText[j * 6 + i].text = "?";
                }
                else
                {
                    rectangleText[j * 6 + i].text = generatedSequences[i][j].ToString();
                }
                sb.Append(rectangleText[j * 6 + i].text + ", ");
            }
            sb.Remove(sb.Length - 2, 2);
            string chosenColour = colours[selectedColours[i]].name;
            chosenColour = chosenColour.Replace("Mat", "");
            Debug.LogFormat("[Missing Sequence #{0}]: {1} Sequence: {2}.", moduleId, chosenColour, sb.ToString());
            Debug.LogFormat("[Missing Sequence #{0}]: {1} Sequence Pattern is {2}.", moduleId, chosenColour, pattern);
            Debug.LogFormat("[Missing Sequence #{0}]: {1} Sequence's Answer: {2}.", moduleId, chosenColour, sequenceAnswers[i].ToString());
            sb.Remove(0, sb.Length);
        }
    }

    IEnumerator countdown()
    {
        isAnimating = true;
        for (int i = 0; i < scrollSpeed.Length; i++) { scrollSpeed[i] = UnityEngine.Random.Range(0.2f, 0.3f); }
        for (int i = 3; i > 0; i--)
        {
            audio.PlaySoundAtTransform("countdown", transform);
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(0.75f);
        }
        countdownText.text = "";
        for (int i = 0; i < rectangles.Length; i++)
        {
            rectangles[i].SetActive(true);
            StartCoroutine(numberScrolls(i));
        }
        isAnimating = false;
    }

    IEnumerator numberScrolls(int k)
    {
        bool firstTime = true;
        float delta = 0;
        float[] y = new float[6] { 119.9271f, 72.9296f, 25.92955f, -21.07042f, -68.07043f, -115.0704f };
        float z = 10.2f;
        while (!moduleSolved)
        {
            if ((k / 6) % 2 == 0)//Odd rows
            {
                if (firstTime && k % 6 == 0)
                {
                    firstTime = false;
                    while (delta < 1)
                    {
                        delta += Time.deltaTime * scrollSpeed[k / 6];
                        rectangles[k].transform.localPosition = new Vector3(Mathf.Lerp(95f, -95f, delta), y[k / 6], z);
                        yield return null;
                    }
                }
                else
                {
                    if (k % 6 == 0 && rectangles[k].transform.localPosition.x - rectangles[k + 5].transform.localPosition.x >= 54.7598f)
                    {
                        while (delta < 1)
                        {
                            delta += Time.deltaTime * scrollSpeed[k / 6];
                            rectangles[k].transform.localPosition = new Vector3(Mathf.Lerp(95f, -95f, delta), y[k / 6], z);
                            yield return null;
                        }
                    }
                    else if (k % 6 != 0 && rectangles[k].transform.localPosition.x - rectangles[k - 1].transform.localPosition.x >= 54.7598f)
                    {
                        if (firstTime && (k % 6 == 4 || k % 6 == 5))//A fix for a weird bug that causes the fifth and sixth rectangles to overlap with the first
                        {
                            firstTime = false;
                        }
                        else
                        {
                            while (delta < 1)
                            {
                                delta += Time.deltaTime * scrollSpeed[k / 6];
                                rectangles[k].transform.localPosition = new Vector3(Mathf.Lerp(95f, -95f, delta), y[k / 6], z);
                                yield return null;
                            }
                        } 
                    }
                }
                rectangles[k].transform.localPosition = new Vector3(95f, y[k / 6], z);
            }
            else//Even rows
            {
                if (firstTime && k % 6 == 0)
                {
                    firstTime = false;
                    while (delta < 1)
                    {
                        delta += Time.deltaTime * scrollSpeed[k / 6];
                        rectangles[k].transform.localPosition = new Vector3(Mathf.Lerp(-95f, 95f, delta), y[k / 6], z);
                        yield return null;
                    }
                }
                else
                {
                    if (k % 6 == 0 && rectangles[k + 5].transform.localPosition.x - rectangles[k].transform.localPosition.x >= 54.7598f)
                    {
                        while (delta < 1)
                        {
                            delta += Time.deltaTime * scrollSpeed[k / 6];
                            rectangles[k].transform.localPosition = new Vector3(Mathf.Lerp(-95f, 95f, delta), y[k / 6], z);
                            yield return null;
                        }
                    }
                    else if (k % 6 != 0 && rectangles[k - 1].transform.localPosition.x - rectangles[k].transform.localPosition.x >= 54.7598f)
                    {
                        if (firstTime && (k % 6 == 4 || k % 6 == 5))//A fix for a weird bug that causes the fifth and sixth rectangles to overlap with the first
                        {
                            firstTime = false;
                        }
                        else
                        {
                            while (delta < 1)
                            {
                                delta += Time.deltaTime * scrollSpeed[k / 6];
                                rectangles[k].transform.localPosition = new Vector3(Mathf.Lerp(-95f, 95f, delta), y[k / 6], z);
                                yield return null;
                            }
                        }
                    }
                }
                rectangles[k].transform.localPosition = new Vector3(-95f, y[k / 6], z);
            }
            yield return null;
            delta = 0f;
        }
    }

    IEnumerator maskChange()
    {
        isAnimating = true;
        float x = mask.localScale.x;
        float z = mask.localScale.z;
        float delta = 0f;
        float from = 3.208636f;
        float to = 0f;
        float duration = 1f;
        while (delta < duration)
        {
            isAnimating = true;
            delta += Time.deltaTime;
            mask.localScale = new Vector3(x, Easing.OutSine(delta, from, to, duration), z);
            yield return null;
        }
        mask.localScale = new Vector3(x, 0f, z);
        if (inputMode)
        {
            foreach (GameObject i in rectangles)
            {
                i.SetActive(false);
            }
            for (int i = 0; i < mainRectangles.Length; i++)
            {
                mainRectangles[i].SetActive(true);
            }
            delta = 0f;
            from = 0f;
            to = 3.208636f;
            duration = 1f;
            while (delta < duration)
            {
                isAnimating = true;
                delta += Time.deltaTime;
                mask.localScale = new Vector3(x, Easing.OutSine(delta, from, to, duration), z);
                yield return null;
            }
            mask.localScale = new Vector3(x, 3.208636f, z);
        }
        else if (striked)
        {
            foreach (GameObject i in mainRectangles)
            {
                i.SetActive(false);
            }
            for (int i = 0; i < rectangles.Length; i++)
            {
                rectangles[i].SetActive(true);
            }
            delta = 0f;
            from = 0f;
            to = 3.208636f;
            duration = 1f;
            while (delta < duration)
            {
                isAnimating = true;
                delta += Time.deltaTime;
                mask.localScale = new Vector3(x, Easing.OutSine(delta, from, to, duration), z);
                yield return null;
            }
            mask.localScale = new Vector3(x, 3.208636f, z);
            striked = false;
        }
        isAnimating = false;
    }

    IEnumerator keypadReveal()
    {
        isAnimating = true;
        float delta = 0f;
        float from = inputMode ? -0.0055f : 0f;
        float to = inputMode ? 0f : -0.0055f;
        float duration = 1f;
        while (delta < duration)
        {
            isAnimating = true;
            delta += Time.deltaTime;
            keypad.transform.localPosition = new Vector3(0f, Easing.InSine(delta, from, to, duration), 0f);
            yield return null;
        }
        isAnimating = false;
    }

    IEnumerator startInputMode()
    {
        while (answerChecks[selectedRectangle])
        {
            selectedRectangle++;
            if (selectedRectangle > 5) { selectedRectangle = 0; }
        }
        while (inputMode)
        {
            for (int i = 0; i < mainRectangleText.Length; i++)
            {
                if (!answerChecks[i])
                {
                    mainRectangleText[i].color = new Color(0f, 0f, 0f, 1f);
                }
            }
            for (int j = 0; j < mainRectangleHighlight.Length; j++)
            {
                if (j == selectedRectangle && !answerChecks[j])
                {
                    mainRectangleHighlight[j].SetActive(true);
                }
                else
                {
                    mainRectangleHighlight[j].SetActive(false);
                }
            }
            yield return null;
        }
    }

    //Twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"<!{0} screen> to press the screen, <!{0} set 42 31> to set 42 to a selected rectangle, then set 31 to the next, <!{0} submit 10 23> to set 10 to a selected rectangle, then set 23 to the next, then press the screen, <!{0} select 4> to select the fourth rectangle, or the next rectangle if no value is mentioned";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        if (Regex.IsMatch(command, @"^\s*screen\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            screen.OnInteract();
            yield return null;
        }
        else if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (!inputMode) { yield return "sendtochaterror The module's not in submission mode yet. Command ignored."; yield break; }
            screen.OnInteract();
            yield return null;
        }
        else if (Regex.IsMatch(command, @"^\s*select\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (!inputMode) { yield return "sendtochaterror The module's not in submission mode yet. Command ignored."; yield break; }
            keypadButtons[11].OnInteract();
            yield return null;
        }
        else
        {
            string[] parameters = command.Split(' ');
            List<KMSelectable> presses = new List<KMSelectable>();
            bool negate = false;
            if ((Regex.IsMatch(parameters[0], @"^\s*set\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) || (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)))
            {
                if (!inputMode) { yield return "sendtochaterror The module's not in submission mode yet. Command ignored."; yield break; }
                if (parameters.Length > 7)
                {
                    yield return "sendtochaterror You can only set up to 6 values per command.";
                    yield break;
                }
                else if (parameters.Length == 1)
                {
                    yield return "sendtochaterror There is no value to set.";
                    yield break;
                }
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (parameters[i].Length > 4)
                    {
                        yield return "sendtochaterror You can only set up to 4 digits per value.";
                        yield break;
                    }
                    for (int k = 0; k < 4 - parameters[i].Length; k++)//Clearing the rectangle first
                    {
                        presses.Add(keypadButtons[0]);
                    }
                    for (int j = 0; j < parameters[i].Length; j++)
                    { 
                        if ((j != 0 && parameters[i][j] == '-') || parameters[i] == "-")
                        {
                            yield return "sendtochaterror One of the values is invalid.";
                            yield break; 
                        }
                        else if (parameters[i][j] == '-')
                        {
                            negate = true;
                        }
                        else if (parameters[i][j] - '0' >= 0 || parameters[i][j] - '0' <= 9)
                        {
                            presses.Add(keypadButtons[parameters[i][j] - '0']);
                        }
                        else
                        {
                            yield return "sendtochaterror One of the values is invalid.";
                            yield break;
                        }
                    }
                    if (negate == true) { presses.Add(keypadButtons[10]); negate = false; }
                    presses.Add(keypadButtons[11]);
                }
                foreach (KMSelectable i in presses)
                {
                    i.OnInteract();
                    yield return new WaitForSeconds(0.05f);
                }
                if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    screen.OnInteract();
                    yield return null;
                }
            }
            else if (Regex.IsMatch(parameters[0], @"^\s*select\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                if (parameters.Length > 2) { yield return "sendtochaterror Invalid command."; yield break; }
                int n = 0;
                bool c = int.TryParse(parameters[1], out n);
                if (!c) { yield return "sendtochaterror Invalid command."; yield break; }
                n -= 1;
                if (n < 0 || n > 6) { yield return "sendtochaterror The rectangle to select is invalid."; yield break; }
                if (answerChecks[n]) { yield return "sendtochaterror The rectangle mentioned is not selectable."; yield break; }
                while (selectedRectangle != n)
                {
                    keypadButtons[11].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

    }

    IEnumerator TwitchHandleForcedSolve()
    {
        bool[] submittedNumbers = new bool[6];
        for (int i = 0; i < submittedNumbers.Length; i++)
        {
            submittedNumbers[i] = answerChecks[i];
        }
        while (!moduleSolved)
        {
            if (isAnimating) { yield return null; }
            else if (!inputMode) { screen.OnInteract(); }
            else
            {
                while(!(submittedNumbers.All(x => x)))
                {
                    if (!submittedNumbers[selectedRectangle])
                    {
                        while (isAnimating)
                        {
                            yield return null;
                        }
                        bool negate = false;
                        if (sequenceAnswers[selectedRectangle] < 0) { negate = true; }
                        string s = (Math.Abs(sequenceAnswers[selectedRectangle])).ToString("0000");
                        for (int i = 0; i < s.Length; i++)
                        {
                            keypadButtons[s[i] - '0'].OnInteract();
                            yield return null;
                        }
                        if (negate) { keypadButtons[10].OnInteract(); yield return null; }
                        if (mainRectangleInputs[selectedRectangle] == sequenceAnswers[selectedRectangle])
                        {
                            submittedNumbers[selectedRectangle] = true;
                        }
                        keypadButtons[11].OnInteract();
                        yield return null;
                    }
                    yield return null;
                }
                screen.OnInteract();
            }
            yield return null;
        }
        yield return null;
    }

}
