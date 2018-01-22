using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	public float fieldRotateSpeed = 15f;
	public float cameraMoveSpeed = 10f;
	public float minDistance = 1f;
	public float maxDistance = 100f;

	public int maxX = 10;
	public int maxY = 10;
	public int maxZ = 10;
	public int minesNum = 800;

	public static Camera mainCamera;

	public float cameraShakeDelay = 0f;
	public float cameraShakeDuration = 1.5f;
	public float cameraShakePower = 2f;
	public float cameraShakeAngle = 15f;

	protected bool gameOver;

	protected int minesLeft;

	private GameObject hudCanvas;
	private GameObject menuCanvas;

	protected Vector3 mainCameraInitialPosition;
	protected Quaternion mainCameraInitialRotation;

	protected Vector3 mainCameraLastPosition;
	protected Quaternion mainCameraLastRotation;

	protected bool[,,] map;
	protected BlockController[,,] blocks;
	protected List<BlockController> safeBlocks;

	protected GameObject fieldPrototype;
	protected GameObject blockPrototype;

	protected GameObject field;

	private Toggle audioToggle;

	void Start()
	{
		Input.simulateMouseWithTouches = true;
		mainCamera = Camera.main;
		audioToggle = GameObject.Find("Audio Toggle").GetComponent<Toggle>();
		audioToggle.isOn = InGameData.audioEnabled;
		if (audioToggle.isOn) {
			AudioListener.volume = 1;
		} else {
			AudioListener.volume = 0;
		}
		hudCanvas = GameObject.Find("Hud Canvas");
		menuCanvas = GameObject.Find("Menu Canvas");
		mainCameraInitialPosition = mainCamera.transform.position;
		mainCameraInitialRotation = mainCamera.transform.rotation;
		fieldPrototype = (GameObject)Resources.Load("Prefabs/Field");
		blockPrototype = (GameObject)Resources.Load("Prefabs/Block");

		maxX = InGameData.maxX;
		maxY = InGameData.maxY;
		maxZ = InGameData.maxZ;
		minesNum = InGameData.minesNum;

		AppearanceManager.Init();

		Init();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, 9999f, ~(1 << LayerMask.NameToLayer("TextBlock") | 1 << LayerMask.NameToLayer("MinedBlock")))) {
				if (hitInfo.collider.name.Contains("Block")) {
					BlockController block = hitInfo.collider.transform.GetComponent<BlockController>();
					if (block.GetState() == BlockState.HIDDEN) {
						block.SetState(BlockState.MINED);
					}
				}
			}
		} else if (Input.GetMouseButtonDown(1)) {
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			//~(1 << LayerMask.NameToLayer ("CubeBlock"))
			if (Physics.Raycast(ray, out hitInfo, 9999f, ~(1 << LayerMask.NameToLayer("MinedBlock")))) {
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

		if (Input.GetKey("w")) {
			//field.transform.RotateAround(field.transform.position, Vector3.right, Time.deltaTime * fieldRotateSpeed);
			RotateCameraDownwards();
		} else if (Input.GetKey("s")) {
			//field.transform.RotateAround(field.transform.position, Vector3.left, Time.deltaTime * fieldRotateSpeed);
			RotateCameraUpwards();
		} else if (Input.GetKey("a")) {
			//field.transform.RotateAround(field.transform.position, Vector3.up, Time.deltaTime * fieldRotateSpeed);
			RotateCameraRightwards();
		} else if (Input.GetKey("d")) {
			//field.transform.RotateAround(field.transform.position, Vector3.down, Time.deltaTime * fieldRotateSpeed);
			RotateCameraLeftwards();
		}
		if (Input.GetKey("j")) {
			MoveCameraCloser();
		} else if (Input.GetKey("k")) {
			MoveCameraFurther();
		} else if (Input.GetKey("r")) {
			ResetCameraTransform();
		} else if (Input.GetKey("p")) {
			ResetScene();
		}
	}

	public virtual void Init()
	{
		HideMenu();

		gameOver = false;

		GenerateField();

		hudCanvas.GetComponent<HudController>().Init();
	}

	public void GenerateField()
	{
		field = Instantiate(fieldPrototype);
		field.transform.position = new Vector3(0f, 0f, 0f);

		//map = new bool[maxX, maxY, maxZ];
		safeBlocks = new List<BlockController>();

		/*
		for (int i = 0; i < maxX; i++) {
			for (int j = 0; j < maxY; j++) {
				for (int k = 0; k < maxZ; k++) {
					map[i, j, k] = false;
				}
			}
		}

		for (int i = 0; i < minesNum; i++) {
			bool isMine = true;
			while (isMine) {
				int x = random.Next(maxX);
				int y = random.Next(maxY);
				int z = random.Next(maxZ);
				isMine = map[x, y, z];
				if (!isMine) {
					map[x, y, z] = true;
				}
			}
		}
		*/
		blocks = new BlockController[maxX, maxY, maxZ];

		for (int i = 0; i < maxX; i++) {
			for (int j = 0; j < maxY; j++) {
				for (int k = 0; k < maxZ; k++) {
					GameObject block = Instantiate(blockPrototype);
					block.transform.parent = field.transform;
					block.transform.position = new Vector3(-maxX / 2 + i + 0.5f, -maxY / 2 + j + 0.5f, -maxZ / 2 + k + 0.5f);
					blocks[i, j, k] = block.GetComponent<BlockController>();
				}
			}
		}

		List<BlockController> potentialMines = new List<BlockController>();
		foreach (BlockController block in blocks) {
			potentialMines.Add(block);
		}

		for (int i = 0; i < minesNum; i++) {
			int r = UnityEngine.Random.Range(0, potentialMines.Count);
			potentialMines[r].SetMine(true);
			potentialMines.RemoveAt(r);
		}

		foreach (BlockController block in potentialMines) {
			block.SetMine(false);
			safeBlocks.Add(block);
		}

		for (int i = 0; i < maxX; i++) {
			for (int j = 0; j < maxY; j++) {
				for (int k = 0; k < maxZ; k++) {
					List<BlockController> neighbors = new List<BlockController>();
					for (int a = i - 1; a < i + 2; a++) {
						if (a >= 0 && a < maxX) {
							for (int b = j - 1; b < j + 2; b++) {
								if (b >= 0 && b < maxY) {
									for (int c = k - 1; c < k + 2; c++) {
										if (c >= 0 && c < maxZ) {
											if (a != i || b != j || c != k) {
												neighbors.Add(blocks[a, b, c]);
											}
										}
									}
								}
							}
						}
					}
					blocks[i, j, k].SetNeighbors(neighbors);
					blocks[i, j, k].Init(this, i, j, k);
				}
			}
		}

		minesLeft = minesNum;
	}

	public void RotateCameraUpwards()
	{
		mainCamera.transform.RotateAround(field.transform.position, mainCamera.transform.right, Time.deltaTime * fieldRotateSpeed);
		mainCameraLastPosition = mainCamera.transform.position;
		mainCameraLastRotation = mainCamera.transform.rotation;
	}

	public void RotateCameraDownwards()
	{
		mainCamera.transform.RotateAround(field.transform.position, -mainCamera.transform.right, Time.deltaTime * fieldRotateSpeed);
		mainCameraLastPosition = mainCamera.transform.position;
		mainCameraLastRotation = mainCamera.transform.rotation;
	}

	public void RotateCameraLeftwards()
	{
		mainCamera.transform.RotateAround(field.transform.position, mainCamera.transform.up, Time.deltaTime * fieldRotateSpeed);
		mainCameraLastPosition = mainCamera.transform.position;
		mainCameraLastRotation = mainCamera.transform.rotation;
	}

	public void RotateCameraRightwards()
	{
		mainCamera.transform.RotateAround(field.transform.position, -mainCamera.transform.up, Time.deltaTime * fieldRotateSpeed);
		mainCameraLastPosition = mainCamera.transform.position;
		mainCameraLastRotation = mainCamera.transform.rotation;
	}

	public void MoveCameraCloser()
	{
		Vector3 position = mainCamera.transform.position;
		position += (transform.position - position).normalized * cameraMoveSpeed * Time.deltaTime;
		if ((position - transform.position).sqrMagnitude < minDistance * minDistance) {
			//position = field.transform.position + (field.transform.position - mainCamera.transform.position).normalized * minDistance;
			position = mainCamera.transform.position;
		}
		mainCamera.transform.position = position;
		mainCameraLastPosition = position;
	}

	public void MoveCameraFurther()
	{
		Vector3 position = mainCamera.transform.position;
		position -= (transform.position - position).normalized * cameraMoveSpeed * Time.deltaTime;
		if ((position - transform.position).sqrMagnitude > maxDistance * maxDistance) {
			//position = field.transform.position + (field.transform.position - mainCamera.transform.position).normalized * maxDistance;
			position = mainCamera.transform.position;
		}
		mainCamera.transform.position = position;
		mainCameraLastPosition = position;
	}

	public void ResetCameraTransform()
	{
		mainCamera.transform.rotation = mainCameraInitialRotation;
		mainCamera.transform.position = mainCameraInitialPosition;
	}

	public void ResetScene()
	{
		StopAllCoroutines();
		/*
		foreach (BlockController block in blocks) {
			Destroy(block.gameObject);
		}
		*/
		Destroy(field);

		mainCamera.transform.rotation = mainCameraInitialRotation;
		mainCamera.transform.position = mainCameraInitialPosition;

		Init();
	}

	public bool IsGameOver()
	{
		return gameOver;
	}

	public IEnumerator ExeShakeTask(float distance)
	{
		yield return new WaitForSeconds(cameraShakeDelay);
		mainCameraLastPosition = mainCamera.transform.position;
		mainCameraLastRotation = mainCamera.transform.rotation;
		float duration = cameraShakeDuration;
		while (duration > 0) {
			Transform objectToMove = mainCamera.transform;
			float cameraShakePower = this.cameraShakePower * duration / cameraShakeDuration / distance;
			float cameraShakeAngle = this.cameraShakeAngle * duration / cameraShakeDuration / distance;
			Vector3 newOffset = new Vector3(UnityEngine.Random.Range(-cameraShakePower, cameraShakePower), UnityEngine.Random.Range(-cameraShakePower, cameraShakePower), 0);
			float newRotationOffset = UnityEngine.Random.Range(-cameraShakeAngle, cameraShakeAngle);
			objectToMove.position = mainCameraLastPosition + newOffset;
			objectToMove.rotation = mainCameraLastRotation;
			objectToMove.Rotate(0, 0, newRotationOffset);
			duration -= Time.deltaTime;
			yield return 0;
		}
		mainCamera.transform.position = mainCameraLastPosition;
		mainCamera.transform.rotation = mainCameraLastRotation;
	}

	public virtual void OnBlockMined(BlockController block)
	{
		if (block.IsMine()) {
			OnLose();
		} else {
			if (safeBlocks.Contains(block)) {
				safeBlocks.Remove(block);
			}
			if (safeBlocks.Count == 0) {
				OnWin();
			}
		}
	}

	public virtual void OnBlockFlagged(BlockController block)
	{
		if (gameOver) {
			return;
		}
		minesLeft--;
		hudCanvas.GetComponent<HudController>().OnMinesLeftChanged(minesLeft);
	}

	public virtual void OnBlockDeflagged(BlockController block)
	{
		if (gameOver) {
			return;
		}
		minesLeft++;
		hudCanvas.GetComponent<HudController>().OnMinesLeftChanged(minesLeft);
	}

	public void BackToMenu()
	{
		SceneManager.LoadScene("MenuScene");
	}

	public void NewGame()
	{
		InGameData.menuSceneIntent = InGameData.MenuSceneIntent.Play;
		BackToMenu();
	}

	public void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
	}

	public void OnAudioToggleChanged()
	{
		InGameData.audioEnabled = !audioToggle.isOn;
		if (audioToggle.isOn) {
			AudioListener.volume = 1;
		} else {
			AudioListener.volume = 0;
		}
	}

	public void ShowMenu(string title)
	{
		if (!menuCanvas.activeSelf) {
			menuCanvas.SetActive(true);
		}
		StartCoroutine(ExeShowMenuTask());
		menuCanvas.transform.Find("Title Text").GetComponent<Text>().text = title;
	}

	private IEnumerator ExeShowMenuTask()
	{
		CanvasGroup canvasGroup = menuCanvas.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0;
		float delay = 1.5f;
		float duration = 1.5f;
		yield return new WaitForSeconds(delay);
		while (canvasGroup.alpha < 1) {
			float increase = Time.deltaTime * 1 / duration;
			if (canvasGroup.alpha + increase > 1) {
				canvasGroup.alpha = 1;
			} else {
				canvasGroup.alpha += increase;
			}
			yield return 0;
		}
		canvasGroup.interactable = true;
	}

	public void HideMenu()
	{
		if (menuCanvas.activeSelf) {
			menuCanvas.SetActive(false);
		}
		menuCanvas.GetComponent<CanvasGroup>().interactable = false;
	}

	private void OnWin()
	{
		if (gameOver) {
			return;
		}

		gameOver = true;

		ShowMenu("You Win !");
	}

	private void OnLose()
	{
		if (gameOver) {
			return;
		}

		gameOver = true;

		ShowMenu("You Lose !");
	}

	private void OnGiveUp()
	{
		if (gameOver) {
			return;
		}

		gameOver = true;

	}
}