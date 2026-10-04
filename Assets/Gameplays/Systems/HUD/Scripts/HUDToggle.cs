using UnityEngine;

public class HUDToggle : MonoBehaviour
{
    public enum Direction {
        XPosition,
        YPosition
    }
    public float hidePosition;
    public Direction hideDirection;
    public bool alwaysShow = false;
    public bool isTime;

    private bool toggle = false; //true＝表示、false＝非表示
    // private float toggleTime = 5f;
    private float timeUntilHide = 5f;
    private RectTransform child;

    private HUDManager manager;

    public int value;
    private int currentValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = FindAnyObjectByType<HUDManager>();
        // toggleTime = 5f;
        if (isTime) {
            timeUntilHide = 0f;
        } else {
            timeUntilHide = 5f;
        }
        child = this.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        if (hideDirection == Direction.YPosition) {
            child.localPosition = Vector3.up * hidePosition;
        } else {
            child.localPosition = Vector3.right * hidePosition;
        }

        currentValue = value;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.ready) {
            return;
        }
        if (alwaysShow) {
            if (timeUntilHide < 2f) {
                timeUntilHide = 2f;
            }
        } else {
            if (timeUntilHide > 0f) {
                timeUntilHide -= Time.deltaTime;
                manager.toggleTime = 5f;
            }
        }

        toggle = (manager.toggleTime <= 0f && !isTime) || timeUntilHide > 0f || GameManager.isPaused || (GameManager.StageClear() && !isTime);

        if (toggle) {
            child.localPosition = Vector3.Lerp(child.localPosition, Vector3.zero, 0.15f);
        } else {
            if (hideDirection == Direction.YPosition) {
                child.localPosition = Vector3.Lerp(child.localPosition, Vector3.up * hidePosition, 0.15f);
            } else {
                child.localPosition = Vector3.Lerp(child.localPosition, Vector3.right * hidePosition, 0.15f);
            }
        }

        if (value != currentValue) {
            ShowThis();
            currentValue = value;
        }
    }

    public void ShowThis() {
        if (timeUntilHide < 2f) {
            timeUntilHide = 2f;
        }
    }
}
