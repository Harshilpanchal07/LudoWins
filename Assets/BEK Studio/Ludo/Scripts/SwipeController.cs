using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BEKStudio;

public class SwipeController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private int totalPages = 5; // Set total number of pages
    [SerializeField] private float tweenTime = 0.3f; // Smooth scrolling time

    [Header("Buttons")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject previousButton;

    private float[] positions; // Normalized positions for pages
    private int currentPage = 0; // Track current page

    private void Start()
    {
        CalculatePagePositions();
        DisableButton(previousButton);
    }

    void CalculatePagePositions()
    {
        positions = new float[totalPages];
        for (int i = 0; i < totalPages; i++)
        {
            positions[i] = (float)i / (totalPages - 1);
        }
    }

    public void DisableButton(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    public void EnableButton(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            MoveToPage(currentPage);
            AudioController.Instance.PlaySFX(AudioController.Instance.SwipeClip);
        }
        if (currentPage == totalPages - 1)
            DisableButton(nextButton);
        EnableButton(previousButton);
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            MoveToPage(currentPage);
            AudioController.Instance.PlaySFX(AudioController.Instance.SwipeClip);
        }
        if (currentPage == 0)
            DisableButton(previousButton);
        EnableButton(nextButton);
    }

    void MoveToPage(int pageIndex)
    {
        float targetPosition = positions[pageIndex];
        LeanTween.value(gameObject, scrollRect.horizontalNormalizedPosition, targetPosition, tweenTime)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) => scrollRect.horizontalNormalizedPosition = val);
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        float closestPos = positions[0];
        int closestIndex = 0;

        for (int i = 0; i < positions.Length; i++)
        {
            if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - positions[i]) < Mathf.Abs(scrollRect.horizontalNormalizedPosition - closestPos))
            {
                closestPos = positions[i];
                closestIndex = i;
            }
        }

        currentPage = closestIndex;
        MoveToPage(currentPage);

        //  Requirement 1: Play swipe sound
        AudioController.Instance.PlaySFX(AudioController.Instance.SwipeClip);

        //  Requirement 2: Enable/Disable buttons like in button functions
        if (currentPage == 0)
        {
            DisableButton(previousButton);
            EnableButton(nextButton);
        }
        else if (currentPage == totalPages - 1)
        {
            DisableButton(nextButton);
            EnableButton(previousButton);
        }
        else
        {
            EnableButton(previousButton);
            EnableButton(nextButton);
        }
    }

}

