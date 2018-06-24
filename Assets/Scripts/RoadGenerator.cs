using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour {

	public RoadSegment[] roadSegments;

	private Vector3 segmentStartPosition;
	private Vector3 segmentStartRotation;

	private Queue<Transform> roadSegmentsQueue;

	void Awake () {

		roadSegmentsQueue = new Queue<Transform>();
		segmentStartPosition = new Vector3(0, 0, 0);
		segmentStartRotation = new Vector3(0, 0, 0);

		for (int i = 0; i < 100; i++) {

			Debug.Log("-----------------------------");
			Debug.Log("Position: " + segmentStartPosition);
			Debug.Log("Rotation: " + segmentStartRotation);

			// int randInt = Random.Range(0, roadSegments.Length)
			int randInt = Random.Range(0, 5);

			RoadSegment newRoadSegment;

			if (randInt > roadSegments.Length - 1) {
				newRoadSegment = roadSegments[0];

			} else {
				newRoadSegment = roadSegments[randInt];
			}
			

			GameObject newSegment = Instantiate<GameObject>(newRoadSegment.prefab);


			// Transform new gameObject
			newSegment.transform.position = segmentStartPosition + newRoadSegment.standardPosition;
			newSegment.transform.eulerAngles = segmentStartRotation + newRoadSegment.standardRotation;
			newSegment.transform.localScale = newRoadSegment.standardScale;

			// Update position and rotation
			float angle = -1 * (segmentStartRotation.y * Mathf.PI / 180);  // The -1 is needed since the rotation matrix works in the other way around as the unity rotation system
			// Apply a rotation vectory
			Vector3 deltaMove = new Vector3(newRoadSegment.deltaEndPosition.x * Mathf.Cos(angle) - newRoadSegment.deltaEndPosition.z * Mathf.Sin(angle), 0, newRoadSegment.deltaEndPosition.x * Mathf.Sin(angle) + newRoadSegment.deltaEndPosition.z * Mathf.Cos(angle));

			segmentStartPosition = segmentStartPosition + deltaMove;
			segmentStartRotation = segmentStartRotation + newRoadSegment.deltaEndRotation;

			roadSegmentsQueue.Enqueue(newSegment.transform);
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
