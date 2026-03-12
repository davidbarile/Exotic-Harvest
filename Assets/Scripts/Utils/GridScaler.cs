using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridScaler : MonoBehaviour
{
    [SerializeField] private bool shouldRefreshOnUpdate = false;

    [SerializeField] private bool shouldResizeContentsToFitNicely = false;

    [SerializeField] private bool showDebug = false;

    [SerializeField] private float xScaleRatioModifier = .95f;

    private GridLayoutGroup cachedGrid;

    private Vector2 gridCellSizes;
    private Vector2 gridSpacing;
    private RectOffset gridPadding;
    private int numItemsThatFitInGridRect = 1;
    private bool shouldRefreshOnceFlag = false;

    private float gridWidth = 0;

    private GridLayoutGroup grid
    {
        get
        {
            if (this.cachedGrid == null || this.gridWidth <= 0)
            {
                this.cachedGrid = this.GetComponent<GridLayoutGroup>();
                this.gridCellSizes = this.cachedGrid.cellSize;
                this.gridSpacing = this.cachedGrid.spacing;
                this.gridPadding = this.cachedGrid.padding;

                this.gridWidth = this.cachedGrid.GetComponent<RectTransform>().rect.width;

                this.numItemsThatFitInGridRect = (int)Mathf.Floor(this.gridWidth / this.gridCellSizes.x);

                if (this.shouldResizeContentsToFitNicely)
                    this.gridCellSizes = new Vector2(this.gridWidth / this.numItemsThatFitInGridRect, this.cachedGrid.cellSize.y);
            }

            if (this.gridWidth <= 0)
                this.shouldRefreshOnceFlag = true;

            return this.cachedGrid;
        }
    }

    private void Start()
    {
        //this is all a workaround hack because the GridLayoutGroup is not being found on Start
        this.shouldRefreshOnceFlag = true;
    }

    private void Update()
    {
        if (this.shouldRefreshOnUpdate || this.shouldRefreshOnceFlag)
        {
            this.shouldRefreshOnceFlag = false;
            RefreshGrid();
        }
    }

    private void RefreshGrid()
    {
        if (this.showDebug)
        {
            Debug.Log(this.transform.ShowObjectPath() + " grid width = " + this.grid.GetComponent<RectTransform>().rect.width + " num = " + this.numItemsThatFitInGridRect);
        }

        Vector2 galaxyDims = new Vector2(1440, 2560);

        float scaleRatioX = (float) Screen.width / galaxyDims.x;
        float scaleRatioY = (float) Screen.height / galaxyDims.y;

        if (this.shouldResizeContentsToFitNicely)
            scaleRatioX = this.xScaleRatioModifier;//sorry for the magic number

        this.grid.cellSize = new Vector2(this.gridCellSizes.x * scaleRatioX, this.gridCellSizes.y * scaleRatioY);
        this.grid.spacing = new Vector2(this.gridSpacing.x * scaleRatioX, this.gridSpacing.y * scaleRatioY);
        this.grid.padding = new RectOffset(Mathf.RoundToInt(this.gridPadding.left * scaleRatioX), Mathf.RoundToInt(this.gridPadding.right * scaleRatioX), Mathf.RoundToInt(this.gridPadding.top * scaleRatioY), Mathf.RoundToInt(this.gridPadding.bottom * scaleRatioY));
    }
}
