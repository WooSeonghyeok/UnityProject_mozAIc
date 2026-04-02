using UnityEngine;
public class CreditPopupManager : MonoBehaviour
{
    public GameObject CreditPopup;
    public void OpenCreditPopup() => CreditPopup.SetActive(true);
    public void CloseCreditPopup() => CreditPopup.SetActive(false);
}
