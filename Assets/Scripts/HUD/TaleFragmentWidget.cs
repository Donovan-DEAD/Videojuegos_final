using UnityEngine;
using UnityEngine.UI;

namespace MadnessDreams
{
    /// <summary>
    /// Widget que muestra el número de fragmentos de relato recogidos.
    /// </summary>
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
