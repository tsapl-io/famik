using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using CP.ProChart;

///<summary>
/// Demo for canvas based bar and line chart using 2D data set
///</summary>
public class GraphScript : MonoBehaviour 
{
    //line chart datas
    public LineChart lineChart;

    //tooltip items
    public RectTransform tooltip;
    public Text tooltipText;

    //labels
    public GameObject labelLine;/*
    public GameObject axisXLabel;
    public GameObject axisYLabel;*/

    //2D Data set
    private ChartData2D dataSet;

    //selection of data
    private int row = -1;
    private int column = -1;
    private int overRow = -1;
    private int overColumn = -1;

    private List<Text> lineLabels = new List<Text>();
    private List<Text> lineXLabels = new List<Text>();
    private List<Text> lineYLabels = new List<Text>();

    public Dropdown HumanDropdown;
    SaveData inStorageData;



    public void OnSelectDelegate(int row, int column)
    {
        this.row = row;
        this.column = column;
    }

    public void OnOverDelegate(int row, int column)
    {
        overRow = row;
        overColumn = column;
    }

    public void Start() 
    {
        if (PlayerPrefs.GetString("Famik", "NO DATA") != "NO DATA" && JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik", "NO DATA")).Humans.Length != 0) {

            inStorageData = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Famik"));


            foreach ( Text n in lineLabels ) print(n.text);
            foreach ( Transform n in lineChart.gameObject.transform )
            {
                Destroy(n.gameObject);
            }
            foreach ( Text n in lineLabels ) print(n.text);

            lineLabels = new List<Text>();
            lineXLabels = new List<Text>();
            lineYLabels = new List<Text>();

            foreach ( Text n in lineLabels ) print(n.text);
            print(lineLabels.Count);

            dataSet = new ChartData2D();
            //#if DEBUG
            //dataSet[1, 0] = 40.0f;
            //#else
            for (int i = 0; i < inStorageData.Humans[HumanDropdown.value].OneSicks.Length; i++)
            {
                dataSet[0, i] = float.NaN;
                dataSet[1, i] = float.Parse(inStorageData.Humans[HumanDropdown.value].OneSicks[i].Fever);
            }
            //#endif

            lineChart.SetValues(ref dataSet);

            lineChart.onSelectDelegate += OnSelectDelegate;
            lineChart.onOverDelegate += OnOverDelegate;

            labelLine.SetActive(true);

            for (int i = 0; i < dataSet.Rows; i++)
            {
                for (int j = 0; j < dataSet.Columns; j++)
                {
                    GameObject obj = (GameObject)Instantiate(labelLine);
                    obj.name = "Label" + i + "_" + j;
                    obj.transform.SetParent(lineChart.transform, false);
                    Text t = obj.GetComponentInChildren<Text>();
                    lineLabels.Add(t);
                }
            }

            lineXLabels.Clear();

        }
    }

    void OnDisable()
    {
        lineChart.onSelectDelegate -= OnSelectDelegate;
        lineChart.onOverDelegate -= OnOverDelegate;
    }

    void Update ()
    {
        tooltip.gameObject.SetActive(overRow != -1);
        if (overRow != -1)
        {
            if (Input.mousePosition.x < 300) {
                tooltip.anchoredPosition = (Vector2)Input.mousePosition + new Vector2(0, 30) + tooltip.sizeDelta * tooltip.localScale.x / 2;
            } else {
                tooltip.anchoredPosition = (Vector2)Input.mousePosition + new Vector2(0, 30) - new Vector2(180, -105);
            }
            tooltipText.text = string.Format("{1}/{2} {3}:{4}\n{0}℃", dataSet[overRow, overColumn].ToString("F1"), inStorageData.Humans[HumanDropdown.value].OneSicks[overColumn].Time.Month, inStorageData.Humans[HumanDropdown.value].OneSicks[overColumn].Time.Day, inStorageData.Humans[HumanDropdown.value].OneSicks[overColumn].Time.Hour, inStorageData.Humans[HumanDropdown.value].OneSicks[overColumn].Time.Minute);
        }

        UpdateLabels(); 
    }
    string TempFever;
    public void UpdateLabels()
    {
        for (int j = 0; j < dataSet.Columns; j++)
        {

            LabelPosition labelPos = lineChart.GetLabelPosition(1, j);
            if (labelPos != null)
            {
                if (float.Parse(inStorageData.Humans[HumanDropdown.value].OneSicks[j].Fever) == 0.0f)
                {
                    if (dataSet.Columns == 1) {
                        dataSet[1, j] = 37.0f;
                    } else if (j == 0) {
                        dataSet[1, j] = dataSet[1, j + 1];
                    } else {
                        if (dataSet.Columns != j + 1) {
                            dataSet[1, j] = (dataSet[1, j - 1] + dataSet[1, j + 1]) / 2;
                        } else {
                            dataSet[1, j] = dataSet[1, j - 1];
                        }
                    }
                    TempFever = "未登録";
                }
                else
                {
                    TempFever = labelPos.value.ToString("0.0") + "℃";
                }
                string minutstemp;
                if (inStorageData.Humans[HumanDropdown.value].OneSicks[j].Time.Minute < 10) {
                    minutstemp = "0" + inStorageData.Humans[HumanDropdown.value].OneSicks[j].Time.Minute.ToString();

                }
                else
                {
                    minutstemp = inStorageData.Humans[HumanDropdown.value].OneSicks[j].Time.Minute.ToString();

                }
                lineLabels[dataSet.Columns + j].text = inStorageData.Humans[HumanDropdown.value].OneSicks[j].Time.Month + "/" + inStorageData.Humans[HumanDropdown.value].OneSicks[j].Time.Day + " " + inStorageData.Humans[HumanDropdown.value].OneSicks[j].Time.Hour + ":" + minutstemp + "\n\n" + TempFever;
                lineLabels[dataSet.Columns + j].color = Color.black;
                lineLabels[dataSet.Columns + j].fontStyle = FontStyle.Bold;
                lineLabels[dataSet.Columns + j].fontSize = 30;
                lineLabels[dataSet.Columns + j].rectTransform.localScale = new Vector3(1, 1, 1);
                lineLabels[dataSet.Columns + j].rectTransform.anchoredPosition = labelPos.position;
                //lineLabels[dataSet.Columns + j].rectTransform.position -= new Vector3(0, 50, 0);
                lineLabels[dataSet.Columns + j].rectTransform.sizeDelta = new Vector2(180, 500);
            }
        }
    }
}