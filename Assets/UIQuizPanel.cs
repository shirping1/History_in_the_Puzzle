using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UIQuizPanel : UIBase
{
    [SerializeField]
    private Button yesButton;
    [SerializeField]
    private Button noButton;
    [SerializeField]
    private Text questionText;

    private int viewID;
    private QuizData currentQuiz;
    private int characterIndex;

    public void Init(int viewID, Person person)
    {
        this.viewID = viewID;
        characterIndex = (int)person;

        yesButton.onClick.AddListener(OnClickedYesButton);
        noButton.onClick.AddListener(OnClickedNoButton);

        currentQuiz = DataManager.Instance.GetNextQuizForCharacter(characterIndex);
        
        if (currentQuiz != null)
        {
            questionText.text = currentQuiz.description;
        }
        else
        {
            questionText.text = "더 이상 문제가 없습니다.";
            yesButton.interactable = false;
            noButton.interactable = false;
        }
    }

    public void OnClickedYesButton()
    {
        // "O"가 정답인 경우에만 맞은 것
        bool isCorrect = currentQuiz.answer == "Y";

        if (isCorrect)
        {
            DataManager.Instance.MarkQuizAsCompleted(characterIndex, currentQuiz.index);
            Debug.Log("정답입니다! 문제 완료 처리");
            LockPuzzlePiece(viewID);
            UIManager.Instance.ClosePeekUI();
        }
        else
        {
            Debug.Log("오답입니다!");
            ResetPuzzlePiece(viewID);
            UIManager.Instance.ClosePeekUI();
        }
    }

    public void OnClickedNoButton()
    {
        // "O"가 정답인 경우에만 맞은 것
        bool isCorrect = currentQuiz.answer == "N";

        if (isCorrect)
        {
            DataManager.Instance.MarkQuizAsCompleted(characterIndex, currentQuiz.index);
            Debug.Log("정답입니다! 문제 완료 처리");
            LockPuzzlePiece(viewID);
            UIManager.Instance.ClosePeekUI();
        }
        else
        {
            Debug.Log("오답입니다!");
            ResetPuzzlePiece(viewID);
            UIManager.Instance.ClosePeekUI();
        }
    }

    public void LockPuzzlePiece(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            RPC_Handler.Instance.photonView.RPC(nameof(RPC_Handler.Instance.RPC_LockPuzzlePiece), RpcTarget.MasterClient, viewID);
            return;
        }

        PhotonView piecePV = PhotonView.Find(viewID);
        if (piecePV != null)
        {
            Piece piece = piecePV.GetComponent<Piece>();
            if (piece != null)
            {
                piece.LockPiece();  // 기존에 구현된 고정 함수 호출
                Debug.Log($"퍼즐 조각 [{piece.row}, {piece.col}] 고정 완료");
            }
            else
            {
                Debug.LogWarning("Piece 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"viewID {viewID}에 해당하는 PhotonView를 찾지 못했습니다.");
        }
    }

    public void ResetPuzzlePiece(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            RPC_Handler.Instance.photonView.RPC(nameof(RPC_Handler.Instance.RPC_ResetPuzzlePiece), RpcTarget.MasterClient, viewID);
            return;
        }

        PhotonView piecePV = PhotonView.Find(viewID);
        if (piecePV != null)
        {
            Piece piece = piecePV.GetComponent<Piece>();
            if (piece != null)
            {
                piece.ResetToRandomPosition();  // 기존에 구현된 고정 함수 호출
                Debug.Log($"퍼즐 조각 [{piece.row}, {piece.col}] 고정 완료");
            }
            else
            {
                Debug.LogWarning("Piece 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"viewID {viewID}에 해당하는 PhotonView를 찾지 못했습니다.");
        }
    }
}
