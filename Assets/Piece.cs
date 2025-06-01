using UnityEngine;

public class Piece : MonoBehaviour
{
    public Vector3 correctPosition;

    // ���� �巡��/Ʈ���� �̺�Ʈ ��� ����� ��ġ �񱳿� �޼���
    public bool IsInCorrectPosition(float threshold = 0.1f)
    {
        return Vector3.Distance(transform.position, correctPosition) < threshold;
    }
}
