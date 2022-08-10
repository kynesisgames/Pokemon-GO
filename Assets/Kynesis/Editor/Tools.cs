using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kynesis.Editor
{
    public class Tools : MonoBehaviour
    {
        private const string SCREENSHOT_FOLDER = "Screenshots";
        private const string SCREENSHOT_FILE = "Screenshot_";
        private const string SCREENSHOT_EXTENSION = ".png";
        
        [MenuItem("Tools/Kynesis/Take Screenshot")]
        private static void TakeScreenshot()
        {
            if (!Directory.Exists(SCREENSHOT_FOLDER))
                Directory.CreateDirectory(SCREENSHOT_FOLDER);

            int number = Directory.GetFiles(SCREENSHOT_FOLDER).Length;
            string fileName = SCREENSHOT_FILE + number.ToString("000") + SCREENSHOT_EXTENSION;
            string fullPath = Path.Combine(SCREENSHOT_FOLDER, fileName);
            
            ScreenCapture.CaptureScreenshot(fullPath);
        }
        
        [MenuItem("Tools/Kynesis/Randomize Children Scale")]
        private static void RandomizeChildrenScale()
        {
            foreach (GameObject selectedGameObject in Selection.gameObjects)
            {
                foreach (Transform child in selectedGameObject.transform)
                {
                    float randomX = Random.Range(1f, 1f);
                    float randomY = Random.Range(1f, 1f);
                    float randomZ = Random.Range(0.8f, 1.2f);
                    Vector3 currentScale = child.transform.localScale;
                    Vector3 randomScale = new Vector3
                    (
                        currentScale.x * randomX,
                        currentScale.y * randomY,
                        currentScale.z * randomZ
                    );
                    child.transform.localScale = randomScale;
                } 
            }
        }
    }
}
