using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    public class TaleFragmentWidget : MonoBehaviour
    {
        [SerializeField] private Text countLabel;

        public void SetCount(int count)
        {
            if (countLabel != null)
            {
                countLabel.text = count.ToString();
            }
        }
    }
}
