using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroll : MonoBehaviour
{
    //参照：https://uuma-memo.xyz/2023/12/18/571/
    [SerializeField, Range(-1, 1)]
	float m_scrollSpeedX = 1.0f;
	[SerializeField, Range(-1, 1)]
	float m_scrollSpeedY = 1.0f;

	Image m_image;
	Material m_material;

	void Start()
	{
		m_image = gameObject.GetComponent<Image>();

		// 他に影響を与えないためにマテリアルを複製
		m_material = new Material(m_image.material);
		m_image.material = m_material;
	}

	void Update()
	{
		if (m_image == null || m_material == null)
			return;

		// 現在のオフセットを取得
		Vector2 offset = m_image.material.mainTextureOffset;

		// X軸のスクロール
		if (m_scrollSpeedX != 0.0f)
		{
			offset.x += m_scrollSpeedX * Time.deltaTime;
		}

		// Y軸のスクロール
		if (m_scrollSpeedY != 0.0f)
		{
			offset.y += m_scrollSpeedY * Time.deltaTime;
		}

		// 更新したオフセットをマテリアルに適用
		m_image.material.mainTextureOffset = offset;
	}

	private void OnDestroy()
	{
		// マテリアルの消去
		Destroy(m_material);
	}
}