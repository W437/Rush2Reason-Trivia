using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TriviaDisplay : MonoBehaviour
{
    public Text questionText;
    public List<Button> answerButtons;

    public void DisplayQuestion(Question question)
    {
        questionText.text = question.question;
        for (int i = 0; i < question.answers.Count; i++)
        {
            answerButtons[i].GetComponentInChildren<Text>().text = question.answers[i];
        }
    }
}
