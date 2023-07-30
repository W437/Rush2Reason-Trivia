using System.Collections.Generic;

public class Question
{
    public string question;
    public List<string> answers;
    public int correctIndex;

    public Question(string squestion, List<string> answers, int correctIndex)
    {
        this.question = question;
        this.answers = answers;
        this.correctIndex = correctIndex;
    }

}