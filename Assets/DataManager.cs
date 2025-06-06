using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class QuizData
{
    public int index;
    public int characterIndex;
    public string description;
    public string answer;
}

public enum Person
{
    DangunWanggeom,
    GwanggaetotheGreat,
    Kanggamchan,
    MAX
}

public class DataManager : MonoBehaviour
{
    [SerializeField] private TextAsset csvFile;

    private List<QuizData> allQuizzes = new();
    private Dictionary<int, List<QuizData>> quizzesByCharacter = new();
    private Dictionary<int, HashSet<int>> completedQuizIndices = new();

    public static DataManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        LoadQuizDataFromCSV();
    }

    void LoadQuizDataFromCSV()
    {
        string[] lines = csvFile.text.Split('\n');
        foreach (string line in lines.Skip(1)) // skip header
        {
            string[] tokens = line.Split(',');

            if (tokens.Length < 4) continue;

            QuizData data = new QuizData
            {
                index = int.Parse(tokens[0]),
                characterIndex = int.Parse(tokens[1]),
                description = tokens[2].Trim(),
                answer = tokens[3].Trim()
            };

            allQuizzes.Add(data);

            if (!quizzesByCharacter.ContainsKey(data.characterIndex))
                quizzesByCharacter[data.characterIndex] = new List<QuizData>();

            quizzesByCharacter[data.characterIndex].Add(data);
        }
    }
    public QuizData GetNextQuizForCharacter(int characterIndex)
    {
        if (!quizzesByCharacter.ContainsKey(characterIndex))
            return null;

        if (!completedQuizIndices.ContainsKey(characterIndex))
            completedQuizIndices[characterIndex] = new HashSet<int>();

        List<QuizData> availableQuizzes = quizzesByCharacter[characterIndex]
            .Where(q => !completedQuizIndices[characterIndex].Contains(q.index))
            .ToList();

        if (availableQuizzes.Count == 0)
            return null;

        // 랜덤으로 하나 선택
        return availableQuizzes[Random.Range(0, availableQuizzes.Count)];
    }

    public void MarkQuizAsCompleted(int characterIndex, int quizIndex)
    {
        if (!completedQuizIndices.ContainsKey(characterIndex))
            completedQuizIndices[characterIndex] = new HashSet<int>();

        completedQuizIndices[characterIndex].Add(quizIndex);
    }

    public void ResetCompletedQuizzes()
    {
        completedQuizIndices.Clear();
    }

    public bool IsAllQuizCompletedForCharacter(int characterIndex)
    {
        if (!quizzesByCharacter.ContainsKey(characterIndex))
            return true;

        if (!completedQuizIndices.ContainsKey(characterIndex))
            return false;

        return completedQuizIndices[characterIndex].Count >= quizzesByCharacter[characterIndex].Count;
    }

    public List<int> GetUnfinishedCharacterIndices()
    {
        List<int> unfinished = new();

        foreach (var kvp in quizzesByCharacter)
        {
            int characterIndex = kvp.Key;

            if (!IsAllQuizCompletedForCharacter(characterIndex))
            {
                unfinished.Add(characterIndex);
            }
        }

        return unfinished;
    }

    // 캐릭터의 모든 퀴즈를 '완료'로 표시
    public void MarkAllQuizzesCompletedForCharacter(int characterIndex)
    {
        if (!quizzesByCharacter.ContainsKey(characterIndex))
            return;

        if (!completedQuizIndices.ContainsKey(characterIndex))
            completedQuizIndices[characterIndex] = new HashSet<int>();

        foreach (var quiz in quizzesByCharacter[characterIndex])
        {
            completedQuizIndices[characterIndex].Add(quiz.index);
        }
    }
}
