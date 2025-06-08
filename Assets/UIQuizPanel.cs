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
            questionText.text = "�� �̻� ������ �����ϴ�.";
            yesButton.interactable = false;
            noButton.interactable = false;
        }
    }

    public void OnClickedYesButton()
    {
        // "O"�� ������ ��쿡�� ���� ��
        bool isCorrect = currentQuiz.answer == "Y";

        if (isCorrect)
        {
            DataManager.Instance.MarkQuizAsCompleted(characterIndex, currentQuiz.index);
            Debug.Log("�����Դϴ�! ���� �Ϸ� ó��");
            LockPuzzlePiece(viewID);
            UIManager.Instance.ClosePeekUI();
        }
        else
        {
            Debug.Log("�����Դϴ�!");
            ResetPuzzlePiece(viewID);
            UIManager.Instance.ClosePeekUI();
        }
    }

    public void OnClickedNoButton()
    {
        // "O"�� ������ ��쿡�� ���� ��
        bool isCorrect = currentQuiz.answer == "N";

        if (isCorrect)
        {
            DataManager.Instance.MarkQuizAsCompleted(characterIndex, currentQuiz.index);
            Debug.Log("�����Դϴ�! ���� �Ϸ� ó��");
            LockPuzzlePiece(viewID);
            UIManager.Instance.ClosePeekUI();
        }
        else
        {
            Debug.Log("�����Դϴ�!");
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
                piece.LockPiece();  // ������ ������ ���� �Լ� ȣ��
                Debug.Log($"���� ���� [{piece.row}, {piece.col}] ���� �Ϸ�");
            }
            else
            {
                Debug.LogWarning("Piece ������Ʈ�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning($"viewID {viewID}�� �ش��ϴ� PhotonView�� ã�� ���߽��ϴ�.");
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
                piece.ResetToRandomPosition();  // ������ ������ ���� �Լ� ȣ��
                Debug.Log($"���� ���� [{piece.row}, {piece.col}] ���� �Ϸ�");
            }
            else
            {
                Debug.LogWarning("Piece ������Ʈ�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning($"viewID {viewID}�� �ش��ϴ� PhotonView�� ã�� ���߽��ϴ�.");
        }
    }
}
