using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{


    //these bools help determine if two spawn points that spawned separately and collide should have connections to each other
    public bool topExit;
    public bool bottomExit;
    public bool rightExit;
    public bool leftExit;

    //this determines the openings a spawned room MUST have, for example, if a room has a top opening that spawns a room on top
    //the opening direction is 1, which indicates the spawned room must have a BOTTOM exit in which to travel between the rooms (a top opening always needs a bottom exit adjoining)
    public int openingDirection;
    private RoomTemplates templates;
    private int rand;

    public bool spawning = false; //used to indicate if a room has already begun the spawn process but has not finished;
    public bool spawned = false;
    float waitTime = 5f;
    float destroyTime = 20f;
    public int roomID; //gives the room a random number to compare which spawner is destroyed in case of spawner collisions
    //1 --> need bottom door
    //2 --> need right door
    //3 --> need top door
    //4 --> need left door

    public SpriteRenderer roomSprite;
    Bounds roomBounds;

    bool addedToRooms = false; //if this is added to the rooms and destroyed later, we can use this bool to confirm this when removing it from the list


    void Start()
    {

        Destroy(gameObject, destroyTime);
        templates = GameAssets.i.roomTemplate;
        roomID = templates.spawnerID++;

        TrySpawnRoom();

    }

    public void TrySpawnRoom()
    {
        templates.RequestSpawn(this);
    }
    public void Spawn()
    {

        spawning = true;
        Vector2 spawnPosition = transform.position;

        // ensure only one spawner can place a room here
        if (!templates.CanPlaceRoom(spawnPosition, this))
        {
            Destroy(gameObject); // this spawner loses the race and gets destroyed
            return;
        }

        templates.currentRoomCount++;
        //room spawns multi opening room until it hits threshold then uses 1 opening rooms to seal up the level
        //the rooms are spawned and connected to the room it spawned from

        //spawn starter rooms to ensure branches will meet minimum room requirements (current a third of the room appx)

        if (spawned == false && templates.allCreatedRooms.Count < GameAssets.i.roomTemplate.threshold1)
        {

            if (openingDirection == 1)
            {
                //need to spawn a room with a bottom door;
                rand = Random.Range(0, templates.starterRoomsBottom.Length);
                GameObject room = Instantiate(templates.starterRoomsBottom[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().north == null)
                {
                    room.GetComponent<NavigationNode>().south = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().north = room.GetComponent<NavigationNode>();
                }

            }

            else if (openingDirection == 2)
            {
                //need to spawn a room with a right door;
                rand = Random.Range(0, templates.starterRoomsRight.Length);
                GameObject room = Instantiate(templates.starterRoomsRight[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().west == null)
                {
                    room.GetComponent<NavigationNode>().east = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().west = room.GetComponent<NavigationNode>();
                }
            }

            else if (openingDirection == 3)
            {
                //need to spawn a room with a top door;
                rand = Random.Range(0, templates.starterRoomsTop.Length);
                GameObject room = Instantiate(templates.starterRoomsTop[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().south == null)
                {
                    room.GetComponent<NavigationNode>().north = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().south = room.GetComponent<NavigationNode>();
                }

            }

            else if (openingDirection == 4)
            {
                //need to spawn a room with a left door;
                rand = Random.Range(0, templates.starterRoomsLeft.Length);
                GameObject room = Instantiate(templates.starterRoomsLeft[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().east == null)
                {
                    room.GetComponent<NavigationNode>().west = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().east = room.GetComponent<NavigationNode>();
                }

            }

            AddToCurrentRooms();

            spawned = true;
        }

        
        //add a few 4 way forking rooms after starter rooms to give some complexity to the level
        else if (spawned == false && templates.allCreatedRooms.Count < GameAssets.i.roomTemplate.threshold2)
        {
            GameObject room;
            if (openingDirection == 1)
            {
                rand = Random.Range(0, templates.forkingRooms.Length);
                room = Instantiate(templates.forkingRooms[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().north == null)
                {
                    room.GetComponent<NavigationNode>().south = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().north = room.GetComponent<NavigationNode>();
                }
            }

            else if (openingDirection == 2)
            {
                rand = Random.Range(0, templates.forkingRooms.Length);
                room = Instantiate(templates.forkingRooms[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().west == null)
                {
                    room.GetComponent<NavigationNode>().east = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().west = room.GetComponent<NavigationNode>();
                }
            }

            else if (openingDirection == 3)
            {
                rand = Random.Range(0, templates.forkingRooms.Length);
                room = Instantiate(templates.forkingRooms[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().south == null)
                {
                    room.GetComponent<NavigationNode>().north = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().south = room.GetComponent<NavigationNode>();
                }

            }

            else if (openingDirection == 4)
            {
                rand = Random.Range(0, templates.forkingRooms.Length);
                room = Instantiate(templates.forkingRooms[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().east == null)
                {
                    room.GetComponent<NavigationNode>().west = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().east = room.GetComponent<NavigationNode>();
                }

            }

            AddToCurrentRooms();

            spawned = true;
        }


        //once the branches are setup, place more rooms with less openings to start "clamping" the paths
        else if (spawned == false &&  templates.allCreatedRooms.Count < GameAssets.i.roomTemplate.threshold3)
        {

            GameObject room;
            if (openingDirection == 1)
            {
                //Need to spawn a room with a bottom door;

                
                //if (templates.bottomRooms[rand].GetComponent<NavigationNode>().exitCount == 3 && templates.threeExitBranch < templates.threeExitBranchCap)
                if (templates.twoExitRoomCount < 0)
                {
                    rand = Random.Range(0, templates.bottomRooms.Length);
                    room = Instantiate(templates.bottomRooms[rand], transform.position, Quaternion.identity);
                    templates.twoExitRoomCount++;
                }

                else 
                {

                    rand = Random.Range(0, templates.bottomRooms1.Length);
                    room = Instantiate(templates.bottomRooms1[rand], transform.position, Quaternion.identity);
                }


                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().north == null)
                {
                    room.GetComponent<NavigationNode>().south = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().north = room.GetComponent<NavigationNode>();
                }



            }

            else if (openingDirection == 2)
            {
                //Need to spawn a room with a right door;
                //if (templates.rightRooms[rand].GetComponent<NavigationNode>().exitCount == 3 && templates.threeExitBranch < templates.threeExitBranchCap)
                if (templates.twoExitRoomCount < 0)
                {

                    templates.twoExitRoomCount++;
                    rand = Random.Range(0, templates.rightRooms.Length);
                    room = Instantiate(templates.rightRooms[rand], transform.position, Quaternion.identity);
                }

                else
                {
                    rand = Random.Range(0, templates.rightRooms1.Length);
                    room = Instantiate(templates.rightRooms1[rand], transform.position, Quaternion.identity);
                }

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().west == null)
                {
                    room.GetComponent<NavigationNode>().east = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().west = room.GetComponent<NavigationNode>();
                }
            }

            else if (openingDirection == 3)
            {
                //Need to spawn a room with a top door;
                //if (templates.topRooms[rand].GetComponent<NavigationNode>().exitCount == 3 && templates.threeExitBranch < templates.threeExitBranchCap)
                if (templates.twoExitRoomCount < 0)
                {

                    templates.twoExitRoomCount++;
                    rand = Random.Range(0, templates.topRooms.Length);
                    room = Instantiate(templates.topRooms[rand], transform.position, Quaternion.identity);
                }

                else
                {
                    rand = Random.Range(0, templates.topRooms1.Length);
                    room = Instantiate(templates.topRooms1[rand], transform.position, Quaternion.identity);
                }

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().south == null)
                {
                    room.GetComponent<NavigationNode>().north = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().south = room.GetComponent<NavigationNode>();
                }

            }

            else if (openingDirection == 4)
            {
                //Need to spawn a room with a left door;
                //if (templates.leftRooms[rand].GetComponent<NavigationNode>().exitCount == 3 && templates.threeExitBranch < templates.threeExitBranchCap)
                if (templates.twoExitRoomCount < 0)
                {

                    templates.twoExitRoomCount++;
                    rand = Random.Range(0, templates.leftRooms.Length);
                    room = Instantiate(templates.leftRooms[rand], transform.position, Quaternion.identity);
                }

                else
                {
                    rand = Random.Range(0, templates.leftRooms1.Length);
                    room = Instantiate(templates.leftRooms1[rand], transform.position, Quaternion.identity);
                }
                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().east == null)
                {
                    room.GetComponent<NavigationNode>().west = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().east = room.GetComponent<NavigationNode>();
                }

            }

            AddToCurrentRooms();

            spawned = true;
        }

        // finally start closing rooms off with close ended rooms
        //once the room count is full, the rooms will be tied off artificially with borders when finalized
        else if (spawned == false && GameAssets.i.roomTemplate.currentRooms < GameAssets.i.roomTemplate.threshold4)
        {

            if (openingDirection == 1)
            {
                //Need to spawn a room with a bottom door;
                rand = Random.Range(0, templates.bottomRooms2.Length);
                GameObject room = Instantiate(templates.bottomRooms2[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().north == null)
                {
                    room.GetComponent<NavigationNode>().south = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().north = room.GetComponent<NavigationNode>();
                }

            }

            else if (openingDirection == 2)
            {
                //Need to spawn a room with a right door;
                rand = Random.Range(0, templates.rightRooms2.Length);
                GameObject room = Instantiate(templates.rightRooms2[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().west == null)
                {
                    room.GetComponent<NavigationNode>().east = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().west = room.GetComponent<NavigationNode>();
                }

            }

            else if (openingDirection == 3)
            {
                //Need to spawn a room with a top door;
                rand = Random.Range(0, templates.topRooms2.Length);
                GameObject room = Instantiate(templates.topRooms2[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().south == null)
                {
                    room.GetComponent<NavigationNode>().north = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().south = room.GetComponent<NavigationNode>();
                }

            }

            else if (openingDirection == 4)
            {
                //Need to spawn a room with a left door;
                rand = Random.Range(0, templates.leftRooms2.Length);
                GameObject room = Instantiate(templates.leftRooms2[rand], transform.position, Quaternion.identity);

                //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
                if (this.gameObject.GetComponentInParent<NavigationNode>().east == null)
                {
                    room.GetComponent<NavigationNode>().west = this.transform.GetComponentInParent<NavigationNode>();

                    this.gameObject.GetComponentInParent<NavigationNode>().east = room.GetComponent<NavigationNode>();
                }

            }

            AddToCurrentRooms();
            spawned = true;

        }

    }


    void AddToCurrentRooms()
    {
        templates.currentRooms++;
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other != null &&  other.CompareTag("SpawnPoint"))
        {
            if (other != null && other.GetComponent<RoomSpawner>().roomID > roomID)
            {

                Destroy(gameObject);

            }
        }

        if (other.CompareTag("Event") || other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }

        
    }


    public void SpawnLockedRoom(string direction)
    {

        Debug.Log("Random spawner number locked room A is " + Random.Range(0, 1000));

        if (direction == "North")
        {
            //Need to spawn a room with a bottom door;
            rand = Random.Range(0, templates.bottomRooms2.Length);
            GameObject room = Instantiate(templates.bottomRooms2[rand], transform.position, Quaternion.identity);

            //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
            if (this.gameObject.GetComponentInParent<NavigationNode>().north == null)
            {
                room.GetComponent<NavigationNode>().south = this.transform.GetComponentInParent<NavigationNode>();

                this.gameObject.GetComponentInParent<NavigationNode>().north = room.GetComponent<NavigationNode>();
            }

            Debug.Log("The room I added was " + room.name);

        }

        else if (direction == "East")
        {
            //Need to spawn a room with a right door;
            rand = Random.Range(0, templates.rightRooms2.Length);
            GameObject room = Instantiate(templates.rightRooms2[rand], transform.position, Quaternion.identity);


            //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
            if (this.gameObject.GetComponentInParent<NavigationNode>().west == null)
            {
                room.GetComponent<NavigationNode>().east = this.transform.GetComponentInParent<NavigationNode>();

                this.gameObject.GetComponentInParent<NavigationNode>().west = room.GetComponent<NavigationNode>();
            }

            Debug.Log("The room I added was " + room.name);
        }

        else if (direction == "South")
        {
            //Need to spawn a room with a top door;
            rand = Random.Range(0, templates.topRooms2.Length);
            GameObject room = Instantiate(templates.topRooms2[rand], transform.position, Quaternion.identity);

            //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
            if (this.gameObject.GetComponentInParent<NavigationNode>().south == null)
            {
                room.GetComponent<NavigationNode>().north = this.transform.GetComponentInParent<NavigationNode>();

                this.gameObject.GetComponentInParent<NavigationNode>().south = room.GetComponent<NavigationNode>();
            }

            Debug.Log("The room I added was " + room.name);
        }

        else if (direction == "West")
        {
            //Need to spawn a room with a left door;
            Debug.Log("The west direction room is " + gameObject.transform.parent.name);
            rand = Random.Range(0, templates.leftRooms2.Length);
            GameObject room = Instantiate(templates.leftRooms2[rand], transform.position, Quaternion.identity);

            //if this room is NOT connected to a room already connect it, otherwise the room spawning will be destroyed
            if (this.gameObject.GetComponentInParent<NavigationNode>().east == null)
            {
                room.GetComponent<NavigationNode>().west = this.transform.GetComponentInParent<NavigationNode>();

                this.gameObject.GetComponentInParent<NavigationNode>().east = room.GetComponent<NavigationNode>();
            }

            Debug.Log("The room I added was " + room.name);
        }

        AddToCurrentRooms();

        spawned = true;

    }


}
