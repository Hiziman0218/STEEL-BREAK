using UnityEngine;
using UnityEngine.EventSystems;

public class MenuItemMouseHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private int index;
    private MainMenu mainMenu;

    public void Setup(int idx, MainMenu menu)
    {
        index = idx;
        mainMenu = menu;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mainMenu?.SetCurrentIndex(index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        mainMenu?.SelectMenuFromIndex(index);
    }
}
