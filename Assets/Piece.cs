using UnityEngine;

public class Piece : MonoBehaviour
{
    public int row, col;
    public Vector3 correctPosition;
    private bool isLocked = false;

    public void LockPiece()
    {
        isLocked = true;
        GetComponent<Collider>().enabled = false;
    }

    public void ResetToRandomPosition()
    {
        if (isLocked) return;

        float x = Random.Range(-20f, 20f);
        float z = Random.Range(-20f, 0f);
        transform.position = new Vector3(x, 0, z);
    }
}
