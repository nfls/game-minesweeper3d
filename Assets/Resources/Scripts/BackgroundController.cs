using System.Collections;
using UnityEngine;

public class BackgroundController : GameController {
	void Start() {
		Input.simulateMouseWithTouches = true;
		fieldRotateSpeed = PreferencesManager.fieldRotateSpeed;
		cameraMoveSpeed = PreferencesManager.cameraMoveSpeed;
		mainCamera = Camera.main;
		mainCameraInitialPosition = mainCamera.transform.position;
		mainCameraInitialRotation = mainCamera.transform.rotation;

		fieldPrototype = (GameObject)Resources.Load("Prefabs/Field");
		blockPrototype = ResourcesManager.GetPrefabByName("Block");
		// blockPrototype = (GameObject)Resources.Load("Prefabs/Block");

		Init();
	}

	void Update() {
		if (Input.GetKey("j")) {
			MoveCameraCloser();
		} else if (Input.GetKey("k")) {
			MoveCameraFurther();
		} else if (Input.GetKey("r")) {
			ResetCameraTransform();
		} else if (Input.mouseScrollDelta.y > float.Epsilon) {
			MoveCameraCloser();
		} else if (Input.mouseScrollDelta.y < -float.Epsilon) {
			MoveCameraFurther();
		}
	}

	public override void Init() {
		gameOver = false;

		maxX = UnityEngine.Random.Range(6, 11);
		maxY = UnityEngine.Random.Range(6, 11);
		maxZ = UnityEngine.Random.Range(6, 11);

		int blocksNum = maxX * maxY * maxZ;

		minesNum = UnityEngine.Random.Range((blocksNum * 1 / 10), (blocksNum * 5 / 10));

		GenerateField();

		StartCoroutine(AutoOperate());
	}

	public override void OnBlockMined(BlockController block) {
		if (block.IsMine()) {
			StartCoroutine(OnGameOver());
		} else {
			if (safeBlocks.Contains(block)) {
				safeBlocks.Remove(block);
			}
			if (safeBlocks.Count == 0) {
				StartCoroutine(OnGameOver());
			}
		}
	}

	IEnumerator OnGameOver() {
		yield return new WaitForSeconds(UnityEngine.Random.Range(20f, 30f));
		ResetScene();
	}

	IEnumerator AutoOperate() {
		yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1.5f));
		int r;
		if (gameOver) {
			r = UnityEngine.Random.Range(0, 3);
		} else {
			r = UnityEngine.Random.Range(0, 2);
		}
		switch (r) {
			case 0: StartCoroutine(AutoRotate()); break;
			case 1: AutoClick(); break;
			case 2: StartCoroutine(AutoMove()); break;
		}
	}

	IEnumerator AutoRotate() {
		float duration = UnityEngine.Random.Range(0.5f, 3f);
		int direction = UnityEngine.Random.Range(0, 4);
		while (duration > 0) {
			switch (direction) {
				case 0:
					RotateCameraDownwards();
					break;
				case 1:
					RotateCameraUpwards();
					break;
				case 2:
					RotateCameraRightwards();
					break;
				case 3:
					RotateCameraLeftwards();
					break;
			}
			duration -= Time.deltaTime;
			yield return 0;
		}
		StartCoroutine(AutoOperate());
	}

	IEnumerator AutoMove() {
		float duration = UnityEngine.Random.Range(0.5f, 2f);
		int direction = UnityEngine.Random.Range(0, 2);
		Vector3 position = mainCamera.transform.position;
		while (duration > 0) {
			switch (direction) {
				case 0:
					MoveCameraCloser();
					break;
				case 1:
					MoveCameraFurther();
					break;
			}
			duration -= Time.deltaTime;
			yield return 0;
		}
		StartCoroutine(AutoOperate());
	}

	void AutoClick() {
		if (gameOver) {
			int r = UnityEngine.Random.Range(0, safeBlocks.Count);
			if (safeBlocks[r].GetState() == BlockState.HIDDEN) {
				safeBlocks[r].SetState(BlockState.MINED);
			} else {
				if (safeBlocks[r].GetState() == BlockState.FLAGGED) {
					safeBlocks[r].SetState(BlockState.MARKED);
				} else {
					safeBlocks[r].SetState(BlockState.HIDDEN);
				}
			}
		} else {
			int x = UnityEngine.Random.Range((int)(Screen.width / 5), (int)(Screen.width * 4 / 5) + 1);
			int y = UnityEngine.Random.Range((int)(Screen.height / 5), (int)(Screen.height * 4 / 5) + 1);
			int r = UnityEngine.Random.Range(0, 10);
			if (r < 7) {
				Ray ray = mainCamera.ScreenPointToRay(new Vector3(x, y, 0));
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, 9999f, ~(1 << LayerMask.NameToLayer("TextBlock")))) {
					if (hitInfo.collider.name.Contains("Block")) {
						BlockController block = hitInfo.collider.transform.GetComponent<BlockController>();
						if (block.GetState() == BlockState.HIDDEN) {
							block.SetState(BlockState.MINED);
						}
					}
				}
			} else {
				Ray ray = mainCamera.ScreenPointToRay(new Vector3(x, y, 0));
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, 9999f)) {
					if (hitInfo.collider.name.Contains("Block")) {
						BlockController block = hitInfo.collider.transform.GetComponent<BlockController>();
						if (block.GetState() == BlockState.HIDDEN) {
							block.SetState(BlockState.FLAGGED);
						} else if (block.GetState() == BlockState.FLAGGED) {
							block.SetState(BlockState.MARKED);
						} else if (block.GetState() == BlockState.MARKED) {
							block.SetState(BlockState.HIDDEN);
						}
					}
				}
			}
		}
		StartCoroutine(AutoOperate());
	}

	public override void OnBlockFlagged(BlockController block) {
		return;
	}

	public override void OnBlockDeflagged(BlockController block) {
		return;
	}

	public void OnEnabled() {
		Init();
	}

	public void OnDisabled() {
		StopAllCoroutines();
		/*
				foreach (BlockController block in blocks) {
					Destroy(block.gameObject);
				}
				*/
		Destroy(field);

		mainCamera.transform.rotation = mainCameraInitialRotation;
		mainCamera.transform.position = mainCameraInitialPosition;
	}
}
