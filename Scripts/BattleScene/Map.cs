using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map {

    // Note that camera control relies on the grid being centered at 0. (Note to self, have the mapCenter variable confirm that is true and fix if not)
    // Note that x and y coordinates passed on don't account for where the counting starts (if the counting starts at -7 then passing in 0 is the same as -7)

    private Grid mapGrid; // The grid the tilemap is on
    private Tilemap mapTiles; // The tilemap used
    private GameObject selectionSprite; // The sprite placed over a tile that a character can potentially move to.

    private Bounds mapBounds; // The bounds of the map
    private int mapHeight; // The number of tiles wide the map is
    private int mapWidth; // The number of tiles high the map is
    private int mapHeightMin; // The minimum value of the tile map's height
    private int mapWidthMin; // The minimum value of the tile map's width
    private Vector2 mapCenter; // All the positions within the map

    private Vector2 cellBounds; // The bounds of a single cell
    private float cellHeight; // The height of a single cell
    private float cellWidth; // The width of a single cell

    private string[][] tileTerrain; // The terrain of each of the tiles on the map
    private string[] impassableTerrain; // The terrain strings that are considered impassable
    private string[] twoMoveTerrain; // The terrain that requires two movement to move into
    private string[] oneMoveTerrain; // The terrain that requires one movement to move into
    private string[] specialEffectTerrain; // The terrain that has a special effect (set by the mapcontroller) of some type

    private GameObject[][] tileOccupation; // the name of the character on a tile (check the object's tags to determine allegiance) or null if uninhabited
    private GameObject[] friendlyCharacters; // All friendly characters active on the map
    private GameObject[] enemyCharacters; // All enemy characters active on the map

    private volatile int[][] distance; // Used by one specific function (getPossibleMoves). Keep in mind to try and replace this with a pointer within the function
    private volatile LinkedList<Vector2> movementOptions; // Used by one specific function (getPossibleMoves). Keep in mind to try and replace this with a pointer within the function.

    // Constructor//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public Map(Tilemap mapTilesRef, string[] impassableTerrainRef, string[] oneMoveTerrainRef, 
        string[] twoMoveTerrainRef, string[] specialEffectTerrainRef, GameObject selectionSpriteRef, Camera currentCameraRef) {
        mapTiles = mapTilesRef;
        mapGrid = mapTiles.layoutGrid;
        selectionSprite = selectionSpriteRef;
        mapBounds = mapTiles.localBounds;
        mapCenter = mapBounds.center;
        mapWidth = (mapTiles.size.x) - 1;
        mapHeight = mapTiles.size.y;
        mapWidthMin = mapTiles.cellBounds.xMin;
        mapHeightMin = mapTiles.cellBounds.yMin;
        cellBounds = mapGrid.cellSize;
        cellHeight = cellBounds.y;
        cellWidth = cellBounds.x;
        tileTerrain = setTileTerrain();
        impassableTerrain = impassableTerrainRef;
        oneMoveTerrain = oneMoveTerrainRef;
        twoMoveTerrain = twoMoveTerrainRef;
        specialEffectTerrain = specialEffectTerrainRef;
        tileOccupation = setTileOccupationInitial();
    }

    // Getters/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public Grid getmapGrid() { return mapGrid; }
    public Tilemap getMapTiles() { return mapTiles; }
    public int getMapHeight() { return mapHeight; }
    public int getMapWidth() { return mapWidth; }
    public int getMapHeightMin() { return mapHeightMin; }
    public int getMapWidthMin() { return mapWidthMin; }
    public Bounds getMapBounds() { return mapBounds; }
    public Vector2 getMapCenter() { return mapCenter; }
    public Vector2 getCellBounds() { return cellBounds; }
    public float getCellHeight() { return cellHeight; }
    public float getCellWidth() { return cellWidth; }
    public string[][] getTileTerrain() { return tileTerrain; }
    public GameObject[][] getTileOccupation() { return tileOccupation; }

    // Setters//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public string[][] setTileTerrain() {
        // Create the 2d array of tile terrains
        string[][] returned = new string[mapWidth][];
        for (int i = 0; i < mapWidth; i++) {
            returned[i] = new string[mapHeight];
        }

        // Retrieve the name of the sprite set to each tile (in order to get the terrain
        for (int i = 0; i < mapWidth; i++) {
            for (int j = 0; j < mapHeight; j++) {
                returned[i][j] = mapTiles.GetTile(new Vector3Int(i + mapWidthMin, j + mapHeightMin, 0)).name;
            }
        }
        return returned;
    }

    // Initialize the map contents to everything empty by default
    public GameObject[][] setTileOccupationInitial() {
        // Create the 2d array of gameobject values
        GameObject[][] returned = new GameObject[mapWidth][];
        for (int i = 0; i < mapWidth; i++) {
            returned[i] = new GameObject[mapHeight];
        }
        return returned;
    }

    // Functions//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Set one tile to uninhabited
    public void emptyOneTile(int xLoc, int yLoc) {
        tileOccupation[xLoc][yLoc] = null;
    }

    // Set one tile as inhabited by a specified gameobject
    public void populateOneTile(int xLoc, int yLoc, GameObject occupier) {
        tileOccupation[xLoc][yLoc] = occupier;
    }

    // Retrieve the name of the one occupying a single tile
    public GameObject getOneTileOccupation(int xLoc, int yLoc) {
        return tileOccupation[xLoc][yLoc];
    }

    // Check the terrain of a tile, return the amount of movement required to move into it. (return -1 if impassable and 0 for special effect, -2 indicates failure to locate)
    public int checkTerrain(int xLoc, int yLoc) {
        string terrain = tileTerrain[xLoc][yLoc];
        // Check for one tile motion terrain
        for (int i = 0; i < oneMoveTerrain.Length; i++) {
            if (terrain.Equals(oneMoveTerrain[i])) {
                return 1;
            }
        }
        // Check for two tile motion terrain
        for (int i = 0; i < twoMoveTerrain.Length; i++) {
            if (terrain.Equals(twoMoveTerrain[i])) {
                return 2;
            }
        }
        // Check for impassable tile motion terrain
        for (int i = 0; i < impassableTerrain.Length; i++) {
            if (terrain.Equals(impassableTerrain[i])) {
                return -1;
            }
        }
        // Check for special effect terrain
        for (int i = 0; i < specialEffectTerrain.Length; i++) {
            if (terrain.Equals(specialEffectTerrain[i])) {
                return 0;
            }
        }
        return -2;
    }

    // Get the center point of a specified map coordinate
    public Vector2 getTileCenter(int xLoc, int yLoc) {
        xLoc = mapWidthMin + xLoc;
        yLoc = mapHeightMin + yLoc;
        return mapTiles.GetCellCenterWorld(new Vector3Int(xLoc, yLoc, 0));
    }

    // Based on a set x and y coordinate and the amount of spaces you can travel, determine where you can move to (potentially).
    public LinkedList<Vector2> getPossibleMoves(int movement, int xLoc, int yLoc) {

        // Reset an array of integers representing all the tiles on the map, for each of the tiles, assign the value -1 to show we don't know how
        // much movement will be remaining when the player is on that tile. Beyond that, use a take on the belmann-ford algorithm and change the value
        // from -1 to the player's remaining movement when they hit that tile, it you are observing a tile that has a value not equal to -1, only change
        // it if the new value is greater than the one currently set. If the cost to move to a new tile would bring the player's movement to a value less
        // than 0, end that recursive branch to show you have gone as far as you can in that direction (so essentially use the concept of depth first search).
        distance = new int[mapWidth][];
        for (int i = 0; i < mapWidth; i++) {
            distance[i] = new int[mapHeight];
            for (int j = 0; j < mapHeight; j++) {
                distance[i][j] = -1; // Indicates we don't know the player's remaining movement when he is on this tile
            }
        }
        // Initialize the linked list of grid coordinates we will keep track of
        movementOptions = new LinkedList<Vector2>();
        movementOptions.AddFirst(new Vector2(xLoc, yLoc));

        // Begin the recursive process (make sure you aren't trying to observe a tile that doesn't exist (is out of bounds)
        distance[xLoc][yLoc] = -2; // Indicates the tile is the root node and thus should be skipped over when checking nodes
        recursiveGetPossibleMoves(xLoc, yLoc, movement);

        return movementOptions;

    }

    // Unchecked (add functionality to prevent trying to move to a populated tile
    // Used exclusively by the getPossibleMoves() function for the recursive call
    private int recursiveGetPossibleMoves(int xLoc, int yLoc, int movement) {

        // Check all the adjacent tiles
        int terrainType;
        int newMovement;
        Vector2 first = new Vector2(xLoc + 1, yLoc); //rightmost tile
        Vector2 second = new Vector2(xLoc - 1, yLoc); //leftmost tile
        Vector2 third = new Vector2(xLoc, yLoc + 1); // upper tile
        Vector2 fourth = new Vector2(xLoc, yLoc - 1); //lower tile
        Vector2[] tileArray = { first, second, third, fourth };

        for (int i = 0; i < 4; i++) {
            Vector2 currentTile = tileArray[i];
            if (currentTile.x >= 0 && currentTile.x < mapWidth && currentTile.y >= 0 && currentTile.y < mapHeight) {
                terrainType = checkTerrain((int) currentTile.x, (int) currentTile.y);
                if (getOneTileOccupation((int)currentTile.x, (int)currentTile.y) != null) terrainType = -1; // if the tile is populated, treat it as impassable terrain

                if (terrainType > 0) newMovement = movement - checkTerrain((int)currentTile.x, (int)currentTile.y); // If the terrain type integer is positive you remove that many steps
                else if (terrainType == -1) {
                    distance[(int)currentTile.x][(int)currentTile.y] = -2; ; // Skip over impassable terrain
                    newMovement = -2;
                }
                else newMovement = movement - 1; // Placeholder for special terrain handling (automatically handles it as movement minus one
                // If the new movement when on this tile is less than 0, you can't move here. If the newMovement is less than the distance currently recorded
                // at that tile, then we already have a better method of reaching this tile. If the distance on this tile is less than -1, then this is either
                // the root or impassable terrain, meaning we skip over it.
                if (newMovement >= 0 && newMovement > distance[(int)currentTile.x][(int)currentTile.y] && distance[(int)currentTile.x][(int)currentTile.y] >= -1) {
                    // If the space has not been visited (set to -1) add this tile to the list of possible movement options
                    if (distance[(int)currentTile.x][(int)currentTile.y] == -1) movementOptions.AddLast(new Vector2((int)currentTile.x, (int)currentTile.y));
                    distance[(int)currentTile.x][(int)currentTile.y] = newMovement; // Set the movement on the currently viewed tile to the new distance
                    recursiveGetPossibleMoves((int)currentTile.x, (int)currentTile.y, newMovement); // start a new branch of recursion from this new tile
                }
            }
        }

        return 0;

    }

}
