using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// To run it, add this component to an empty GameObject and press Play.
public class SundaeRush : MonoBehaviour
{
    [Header("Sounds (drag clips in, any can be left empty)")]
    public AudioClip clickSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    // Round settings (each array is Round 1, Round 2, Round 3)
    int[] ordersToWin = { 10, 10, 10 };
    float[] secondsBetweenCustomers = { 8f, 6.5f, 5f };
    float[] customerPatience = { 30f, 25f, 20f };

    string[][] flavors =
    {
        new[] { "Chocolate", "Vanilla", "Strawberry" },
        new[] { "Chocolate", "Vanilla", "Strawberry", "Mint" },
        new[] { "Chocolate", "Vanilla", "Strawberry", "Mint", "Coffee" },
    };
    string[][] sauces =
    {
        new[] { "Chocolate", "Caramel" },
        new[] { "Chocolate", "Caramel", "Strawberry" },
        new[] { "Chocolate", "Caramel", "Strawberry", "Marshmallow" },
    };
    string[][] toppings =
    {
        new[] { "Cream", "Cherry" },
        new[] { "Cream", "Cherry", "Nuts" },
        new[] { "Cream", "Cherry", "Nuts", "Sprinkles" },
    };

    // Text shown on the green story screens
    const string menuText = "<size=96>SUNDAE RUSH</size>";
    const string introText =
        "Welcome to your first day on the job!\n\n" +
        "Our ice cream parlor has fallen on hard times. Make the right sundaes before customers lose their patience.\n\n" +
        "Good orders earn good reviews. 3 bad reviews and we're out of business!";
    const string howToText =
        "HOW TO PLAY\n\n" +
        "Click a flavor, then a sauce, then a topping.\n\n" +
        "Click the sundae, then click the customer who ordered it.\n\n" +
        "Made a mistake? Click the cones to start over.";
    string[] roundText =
    {
        "ROUND 1\n\nFLAVORS: Chocolate, Vanilla, Strawberry\n\nSAUCES: Hot Fudge, Caramel\n\nTOPPINGS: Whipped Cream, Cherries\n\n\nServe 10 happy customers!",
        "ROUND 2\n\nNEW FLAVOR: Mint Chip\n\nNEW SAUCE: Strawberry\n\nNEW TOPPING: Nuts\n\n\nServe 10 happy customers!",
        "ROUND 3\n\nNEW FLAVOR: Coffee\n\nNEW SAUCE: Marshmallow Fluff\n\nNEW TOPPING: Sprinkles\n\n\nServe 10 happy customers!",
    };
    string[] bossText =
    {
        "",
        "BOSS:\n\n\"Great work! Word is getting around town. We can finally afford some new flavors!\"",
        "BOSS:\n\n\"Business is booming! Let's expand the menu again!\"",
    };
    const string winText =
        "BOSS:\n\n\"You did it! Thanks to you, our parlor is the most popular spot in town!\"\n\n\n<size=72>YOU WIN!</size>";
    const string loseText =
        "Oh no... 3 bad reviews.\n\nThe ice cream parlor went out of business.\n\nYou'll have to start over from Round 1.";

    // Clickable areas for each tub, bottle, and jar (x, y, width, height)
    Dictionary<string, Rect> flavorSpots = new Dictionary<string, Rect>
    {
        { "Coffee",     new Rect(150, 790, 118, 130) },
        { "Mint",       new Rect(274, 790, 118, 130) },
        { "Strawberry", new Rect(398, 790, 110, 130) },
        { "Chocolate",  new Rect(515, 790, 112, 130) },
        { "Vanilla",    new Rect(635, 790, 113, 130) },
    };
    Dictionary<string, Rect> sauceSpots = new Dictionary<string, Rect>
    {
        { "Caramel",     new Rect(1272, 690, 58, 95) },
        { "Chocolate",   new Rect(1352, 690, 60, 95) },
        { "Strawberry",  new Rect(1312, 790, 64, 125) },
        { "Marshmallow", new Rect(1393, 790, 63, 125) },
    };
    Dictionary<string, Rect> toppingSpots = new Dictionary<string, Rect>
    {
        { "Cream",     new Rect(1500, 735, 72, 180) },
        { "Cherry",    new Rect(1588, 750, 98, 168) },
        { "Nuts",      new Rect(1692, 750, 98, 168) },
        { "Sprinkles", new Rect(1797, 750, 105, 168) },
    };
    Rect coneSpot = new Rect(0, 715, 130, 205);
    Rect sundaeSpot = new Rect(935, 600, 160, 330);
    float[] customerX = { 300, 860, 1420 };

    // Colors
    Color pastelGreen = new Color32(154, 207, 164, 255);
    Color darkGreen = new Color32(74, 122, 86, 255);
    Color cream = new Color32(255, 245, 214, 255);
    Color brown = new Color32(92, 61, 46, 255);
    Color red = new Color32(200, 40, 40, 255);

    // Values the game tracks while it runs
    class Customer
    {
        public GameObject root;
        public Image body, bubble, scoopIcon, sauceIcon;
        public TextMeshProUGUI timer;
        public string flavor, sauce, topping;
        public float timeLeft;
        public bool leaving;
    }

    TMP_FontAsset font;
    AudioSource audioSource;
    Sprite happyFace, neutralFace, madFace, bubbleCorrect, bubbleIncorrect;

    Transform gameRoot, customerRoot, spotRoot;
    Image background, counter, picture;
    RectTransform sundaeRoot;
    Image scoopImg, sauceImg, toppingImg;
    TextMeshProUGUI roundLabel, reviewLabel, livesLabel, screenText, screenHint;
    RectTransform reviewFill;
    GameObject textScreen;

    Customer[] slots = new Customer[3];
    int round, lives, reviews;
    string scoop, sauce, topping;
    bool sundaeSelected, playing, clicked;

    void Start()
    {
        BuildUI();
        StartCoroutine(GameFlow());
    }

    // Overall game flow: menu, intro, then each round, then the win screen
    IEnumerator GameFlow()
    {
        while (true)
        {
            yield return ShowText(menuText, "CLICK TO START");
            yield return ShowText(introText);
            yield return ShowText(howToText);

            bool won = false;
            while (!won)
            {
                lives = 3; // lives carry over between rounds and only reset on a restart
                bool lost = false;
                for (int r = 0; r < 3 && !lost; r++)
                {
                    if (r > 0) yield return ShowText(bossText[r]);
                    yield return ShowPicture(Art("Background1_Round" + (r + 1)), 3f);
                    yield return ShowText(roundText[r]);
                    yield return PlayRound(r);
                    lost = lives <= 0;
                }
                if (lost) yield return ShowText(loseText, "CLICK TO TRY AGAIN");
                else won = true;
            }
            yield return ShowText(winText, "CLICK TO PLAY AGAIN");
        }
    }

    IEnumerator ShowText(string text, string hint = "CLICK TO CONTINUE")
    {
        screenText.text = text;
        screenHint.text = hint;
        textScreen.SetActive(true);
        yield return new WaitForSeconds(0.3f); // short delay so a double-click doesn't skip a screen
        clicked = false;
        yield return new WaitUntil(() => clicked);
        Play(clickSound);
        textScreen.SetActive(false);
    }

    IEnumerator ShowPicture(Sprite sprite, float seconds)
    {
        picture.sprite = sprite;
        picture.gameObject.SetActive(true);
        yield return new WaitForSeconds(seconds);
        picture.gameObject.SetActive(false);
    }

    IEnumerator PlayRound(int r)
    {
        round = r;
        reviews = 0;
        ClearCustomers();
        ClearSundae();
        background.sprite = Art("Background2_Round" + (r + 1));
        counter.sprite = Art("Counter_Round" + (r + 1));
        BuildClickAreas();
        UpdateHud();
        gameRoot.gameObject.SetActive(true);
        playing = true;

        float nextCustomer = 0f;
        while (reviews < ordersToWin[r] && lives > 0)
        {
            nextCustomer -= Time.deltaTime;
            if (nextCustomer <= 0f && SpawnCustomer())
                nextCustomer = secondsBetweenCustomers[r];

            foreach (Customer c in slots)
            {
                if (c == null || c.leaving) continue;
                c.timeLeft -= Time.deltaTime;
                c.timer.text = Mathf.CeilToInt(c.timeLeft).ToString();
                c.timer.color = c.timeLeft <= 5f ? red : brown;
                c.body.sprite = c.timeLeft < customerPatience[r] / 3f ? madFace : neutralFace;
                if (c.timeLeft <= 0f)
                {
                    lives--; // customer ran out of patience, which counts as a bad review
                    Play(wrongSound);
                    CustomerLeaves(c, false);
                    UpdateHud();
                }
            }
            yield return null;
        }

        playing = false;
        yield return new WaitForSeconds(1.5f);
        ClearCustomers();
        gameRoot.gameObject.SetActive(false);
    }

    // Customers
    bool SpawnCustomer()
    {
        List<int> free = new List<int>();
        for (int i = 0; i < slots.Length; i++) if (slots[i] == null) free.Add(i);
        if (free.Count == 0) return false;

        int slot = free[Random.Range(0, free.Count)];
        float x = customerX[slot];
        Customer c = new Customer();
        c.flavor = Pick(flavors[round]);
        c.sauce = Pick(sauces[round]);
        c.topping = Pick(toppings[round]);
        c.timeLeft = customerPatience[round];

        c.root = Layer(customerRoot, "Customer").gameObject;

        c.body = Img(c.root.transform, neutralFace);
        Place(c.body, x, 800, 0.5f, 0f);
        c.body.transform.localScale = Vector3.one * 0.8f;
        c.body.raycastTarget = true;
        Button b = c.body.gameObject.AddComponent<Button>();
        b.transition = Selectable.Transition.None;
        b.onClick.AddListener(() => ClickCustomer(c));

        // The bubble sprite already includes the topping, so only the scoop and sauce icons are added.
        c.bubble = Img(c.root.transform, Art("Bubble_" + c.topping));
        Place(c.bubble, x + 90, 400, 0f, 0f);
        c.bubble.transform.localScale = Vector3.one * 0.7f;
        c.scoopIcon = Img(c.bubble.transform, Art("Icon_Scoop_" + c.flavor));
        Place(c.scoopIcon, 104, 124);
        c.sauceIcon = Img(c.bubble.transform, Art("Icon_Sauce_" + c.sauce));
        Place(c.sauceIcon, 228, 124);

        c.timer = Label(c.root.transform, "", 40, brown, x - 200, 470, 160, 60);

        slots[slot] = c;
        return true;
    }

    void ClickCustomer(Customer c)
    {
        if (!playing || c.leaving || !sundaeSelected) return;

        bool correct = c.flavor == scoop && c.sauce == sauce && c.topping == topping;
        if (correct) { reviews++; Play(correctSound); }
        else { lives--; Play(wrongSound); }

        CustomerLeaves(c, correct);
        ClearSundae();
        UpdateHud();
    }

    void CustomerLeaves(Customer c, bool happy)
    {
        c.leaving = true;
        c.body.sprite = happy ? happyFace : madFace;
        c.bubble.sprite = happy ? bubbleCorrect : bubbleIncorrect;
        c.scoopIcon.enabled = false;
        c.sauceIcon.enabled = false;
        c.timer.text = "";
        StartCoroutine(RemoveCustomerLater(c));
    }

    IEnumerator RemoveCustomerLater(Customer c)
    {
        yield return new WaitForSeconds(1.2f);
        if (c.root) Destroy(c.root);
        for (int i = 0; i < slots.Length; i++) if (slots[i] == c) slots[i] = null;
    }

    void ClearCustomers()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].root) Destroy(slots[i].root);
            slots[i] = null;
        }
    }

    // Building the sundae (scoop, then sauce, then topping)
    void ClickFlavor(string f)
    {
        if (!playing) return;
        Play(clickSound);
        scoop = f;
        sundaeSelected = false;
        RefreshSundae();
    }

    void ClickSauce(string s)
    {
        if (!playing || scoop == null) return; // a scoop is required before adding sauce
        Play(clickSound);
        sauce = s;
        sundaeSelected = false;
        RefreshSundae();
    }

    void ClickTopping(string t)
    {
        if (!playing || sauce == null) return; // sauce is required before adding a topping
        Play(clickSound);
        topping = t;
        sundaeSelected = false;
        RefreshSundae();
    }

    void ClickCones()
    {
        if (!playing) return;
        Play(clickSound);
        ClearSundae(); // discards the current sundae
    }

    void ClickSundae()
    {
        if (!playing || topping == null) return; // only a finished sundae can be selected
        Play(clickSound);
        sundaeSelected = !sundaeSelected;
        RefreshSundae();
    }

    void ClearSundae()
    {
        scoop = sauce = topping = null;
        sundaeSelected = false;
        RefreshSundae();
    }

    void RefreshSundae()
    {
        SetSprite(scoopImg, scoop == null ? null : Art("Scoop_" + scoop));
        SetSprite(sauceImg, sauce == null ? null : Art("Sauce_" + sauce));
        SetSprite(toppingImg, topping == null ? null : Art("Topping_" + topping));
        sundaeRoot.anchoredPosition = new Vector2(0, sundaeSelected ? 25 : 0); // raises the sundae slightly to show it's selected
    }

    // Top bar (round, reviews, lives)
    void UpdateHud()
    {
        roundLabel.text = "ROUND " + (round + 1);
        reviewLabel.text = reviews + "/" + ordersToWin[round];
        livesLabel.text = "LIVES: " + Mathf.Max(lives, 0);
        reviewFill.anchorMax = new Vector2(Mathf.Clamp01((float)reviews / ordersToWin[round]), 1);
    }

    // Creates all on-screen elements when the game starts
    void BuildUI()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        Font f = Resources.Load<Font>("Fonts/PressStart2P-Regular");
        if (f != null) font = TMP_FontAsset.CreateFontAsset(f);

        happyFace = Art("Customer_Happy");
        neutralFace = Art("Customer_Neutral");
        madFace = Art("Customer_Mad");
        bubbleCorrect = Art("Bubble_Correct");
        bubbleIncorrect = Art("Bubble_Incorrect");

        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Transform canvas = canvasGO.transform;

        // Gameplay layers, created from back to front.
        gameRoot = Layer(canvas, "Game");
        background = FullImage(gameRoot);
        customerRoot = Layer(gameRoot, "Customers");
        counter = FullImage(gameRoot);

        sundaeRoot = (RectTransform)Layer(gameRoot, "Sundae");
        Image cone = Img(sundaeRoot, Art("Cone"));
        Place(cone, 1015, 930, 0.5f, 0f);
        scoopImg = Img(sundaeRoot, null);
        Place(scoopImg, 1015, 775, 0.5f, 0f);
        sauceImg = Img(sundaeRoot, null);
        Place(sauceImg, 1015, 700, 0.5f, 0f);
        toppingImg = Img(sundaeRoot, null);
        Place(toppingImg, 1015, 645, 0.5f, 0f);

        spotRoot = Layer(gameRoot, "ClickAreas");
        ClickArea(gameRoot, coneSpot, ClickCones);
        ClickArea(gameRoot, sundaeSpot, ClickSundae);

        // Background bar for the top HUD.
        Image bar = Box(gameRoot, new Color(0.36f, 0.24f, 0.18f, 0.85f));
        RectTransform barRT = bar.rectTransform;
        barRT.anchorMin = new Vector2(0, 1);
        barRT.anchorMax = new Vector2(1, 1);
        barRT.pivot = new Vector2(0.5f, 1);
        barRT.anchoredPosition = Vector2.zero;
        barRT.sizeDelta = new Vector2(0, 80);

        roundLabel = Label(gameRoot, "", 30, cream, 210, 40, 400, 60);
        Label(gameRoot, "REVIEWS", 26, cream, 610, 40, 300, 60);
        Image meterBack = Box(gameRoot, new Color32(60, 40, 30, 255));
        Place(meterBack, 760, 40, 0f, 0.5f);
        meterBack.rectTransform.sizeDelta = new Vector2(420, 30);
        Image meterFill = Box(meterBack.transform, new Color32(240, 150, 170, 255));
        reviewFill = meterFill.rectTransform;
        reviewFill.anchorMin = Vector2.zero;
        reviewFill.anchorMax = new Vector2(0, 1);
        reviewFill.offsetMin = reviewFill.offsetMax = Vector2.zero;
        reviewLabel = Label(gameRoot, "", 26, cream, 1290, 40, 200, 60);
        livesLabel = Label(gameRoot, "", 30, cream, 1700, 40, 400, 60);

        gameRoot.gameObject.SetActive(false);

        // Full-screen image shown for a few seconds before each round.
        picture = FullImage(canvas);
        picture.raycastTarget = true; // prevents clicks from passing through
        picture.gameObject.SetActive(false);

        // Story screen
        Image screen = FullImage(canvas);
        screen.color = pastelGreen;
        screen.raycastTarget = true;
        Button sb = screen.gameObject.AddComponent<Button>();
        sb.transition = Selectable.Transition.None;
        sb.onClick.AddListener(() => clicked = true);
        screenText = Label(screen.transform, "", 36, cream, 960, 480, 1600, 820);
        screenText.lineSpacing = 20;
        screenText.outlineWidth = 0.2f;
        screenText.outlineColor = darkGreen;
        screenHint = Label(screen.transform, "", 24, cream, 960, 1000, 1400, 60);
        screenHint.outlineWidth = 0.2f;
        screenHint.outlineColor = darkGreen;
        textScreen = screen.gameObject;
        textScreen.SetActive(false);
    }

    void BuildClickAreas()
    {
        foreach (Transform t in spotRoot) Destroy(t.gameObject);
        foreach (string f in flavors[round]) ClickArea(spotRoot, flavorSpots[f], () => ClickFlavor(f));
        foreach (string s in sauces[round]) ClickArea(spotRoot, sauceSpots[s], () => ClickSauce(s));
        foreach (string t in toppings[round]) ClickArea(spotRoot, toppingSpots[t], () => ClickTopping(t));
    }

    // Helper methods
    Sprite Art(string name)
    {
        Sprite s = Resources.Load<Sprite>("Art/" + name);
        if (s == null) Debug.LogWarning("Missing sprite: Resources/Art/" + name);
        return s;
    }

    string Pick(string[] options) => options[Random.Range(0, options.Length)];

    void Play(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    // Creates an empty full-screen container for grouping elements.
    Transform Layer(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Stretch((RectTransform)go.transform);
        return go.transform;
    }

    Image Img(Transform parent, Sprite sprite)
    {
        GameObject go = new GameObject(sprite != null ? sprite.name : "Image", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.raycastTarget = false;
        SetSprite(img, sprite);
        return img;
    }

    // Creates a solid-colored rectangle.
    Image Box(Transform parent, Color color)
    {
        Image img = Img(parent, null);
        img.enabled = true;
        img.color = color;
        return img;
    }

    Image FullImage(Transform parent)
    {
        Image img = Box(parent, Color.white);
        Stretch(img.rectTransform);
        return img;
    }

    void SetSprite(Image img, Sprite sprite)
    {
        img.sprite = sprite;
        img.enabled = sprite != null;
        if (sprite != null) img.SetNativeSize();
    }

    // Positions an element using pixel coordinates from the parent's top-left corner.
    void Place(Component c, float x, float y, float pivotX = 0.5f, float pivotY = 0.5f)
    {
        RectTransform rt = (RectTransform)c.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(pivotX, pivotY);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    TextMeshProUGUI Label(Transform parent, string text, float size, Color color, float x, float y, float w, float h)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        Place(t, x, y);
        t.rectTransform.sizeDelta = new Vector2(w, h);
        return t;
    }

    // Creates an invisible button over part of the artwork.
    void ClickArea(Transform parent, Rect r, UnityAction onClick)
    {
        GameObject go = new GameObject("ClickArea", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        Button b = go.GetComponent<Button>();
        b.transition = Selectable.Transition.None;
        b.onClick.AddListener(onClick);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(r.x, -r.y);
        rt.sizeDelta = new Vector2(r.width, r.height);
    }
}
