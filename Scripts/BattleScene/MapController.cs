using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapController : MonoBehaviour {

    // Not map specific
    public GameObject selectionSprite;
    public Camera currentCamera;
    public float cameraSpeed; // The speed at which the camera moves

    private Map current;
    // The available movement options for the last selected character (automatically updates whenever a new character who can move is selected.
    private LinkedList<Vector2> movementOptions;
    private GameObject[] selectionReticles;

    // Map specific
    //private int maxCharacters = 2;
    // Keep in mind the counting for positions starts from the bottom left
    private Vector2[] friendlyStartPositions;
    private Vector2[] enemyStartPositions;
    public GameObject[] friendlyCharacters; // Remember, if this exceeds the number of start positions, the characters won't be placed
    public GameObject[] enemyCharacters; // Remember, if this exceeds the number of start positions, the characters won't be placed

	// Use this for initialization
	void Start () {

        // Map Specific
        string[] oneMoveTerrain = { "Sand_Tile", "Seashore_LandCorner_Tile", "Seashore_Tile", "Seashore_WaterCorner_Tile" };
        string[] twoMoveTerrain = { "Palm_Tree_Tile" };
        string[] impassableTerrain = { "Water_Tile" };

        friendlyStartPositions = new Vector2[2];
        friendlyStartPositions[0] = new Vector2(16, 4);
        friendlyStartPositions[1] = new Vector2(16, 11);

        enemyStartPositions = new Vector2[4];
        enemyStartPositions[0] = new Vector2(6, 5);
        enemyStartPositions[1] = new Vector2(6, 10);
        enemyStartPositions[2] = new Vector2(15, 4); // 10, 5 originally
        enemyStartPositions[3] = new Vector2(10, 10);

        // Not map specific
        current = new Map(this.GetComponent<Tilemap>(), impassableTerrain, oneMoveTerrain, twoMoveTerrain, null, selectionSprite, currentCamera);
        spawnCharacters();
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Q)) {
            removeSelections();
        }

        if (Input.GetKeyUp(KeyCode.W)) {
            placeSelections();
        }

    }

    // Spawns as many characters from the list of fighting enemies and allies as will fit in the list of available starting spaces for each team
    public void spawnCharacters() {
        for (int i = 0; i < friendlyStartPositions.Length; i++) {
            if (friendlyCharacters[i] != null) {
                Vector2 newPosition = current.getTileCenter((int)friendlyStartPositions[i].x, (int)friendlyStartPositions[i].y);
                GameObject newCharacter = Instantiate<GameObject>(friendlyCharacters[i], new Vector3(newPosition.x, newPosition.y, 0), friendlyCharacters[i].transform.rotation);
                current.populateOneTile((int) friendlyStartPositions[i].x, (int)friendlyStartPositions[i].y, newCharacter);
            }
        }
        for (int i = 0; i < enemyStartPositions.Length; i++) {
            if (enemyCharacters[i] != null) {
                Vector2 newPosition = current.getTileCenter((int) enemyStartPositions[i].x, (int) enemyStartPositions[i].y);
                GameObject newCharacter = Instantiate<GameObject>(enemyCharacters[i], new Vector3(newPosition.x, newPosition.y, 0), enemyCharacters[i].transform.rotation);
                current.populateOneTile((int) enemyStartPositions[i].x, (int) enemyStartPositions[i].y, newCharacter);
            }
        }
    }

    // unchecked
    // Place a selection reticle over all possible movement spaces
    public void placeSelections() {

        selectionSprite.SetActive(true);

        movementOptions = current.getPossibleMoves(7, 16, 4);
        LinkedList<Vector2>.Enumerator movementOptionsEnum = movementOptions.GetEnumerator();
        selectionReticles = new GameObject[movementOptions.Count];

        GameObject newest;
        int count = -1;
        while (movementOptionsEnum.MoveNext()) {
            count++;
            Vector2 tileCenter = current.getTileCenter((int) movementOptionsEnum.Current.x, (int) movementOptionsEnum.Current.y);
            newest = Instantiate<GameObject>(selectionSprite, new Vector3(tileCenter.x, tileCenter.y, 0), selectionSprite.transform.rotation);
            selectionReticles[count] = newest;
        }

        selectionSprite.SetActive(false);
    }

    // unchecked
    // Remove the selection reticles that were placed
    public void removeSelections() {
        for (int i = 0; i < selectionReticles.Length; i++) {
            Destroy(selectionReticles[i]);
        }
        selectionReticles = null;
    }

}
