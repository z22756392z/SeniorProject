# SeniorProject

執行專案之前需要添加或修改的步驟: [Addon](Addon.md)

[Database](./Assets/DataBase/README.md)

## Log

> 7/10

* Add
  * Holistic Scene

>  7/26
* Add
  * Acupuncture point SO

> 7/28
* Add
  * EditorScript -- update inventory
  
  * Acupuncture point Scene (在執行之前記得依序跟新穴道位置和背包有時候資料會跑掉 (在MenuItem的SeniorProject中的 Update acupunture point ScriptableObjects 和 Add all acupunture point to default Inventory))
  
    ![image-20220728161249483](./Image/LeftHandAcupturePoint.png)
  
  * Acupuncutre Solution
  

> 8/2
* add 
  * rasa scene and some art
  * add new script `BotUI.cs`、 `NetworkManager`...
  * add rasa file，in order to run rasa server
* fix
  * fix memory leak bug

> 8/3
* add 
  * add new func to show the return value from AI

TODO: Tutorial UI、syn，asyn、Debug Canvas(什麼時間跑出什麼字串)

> 8/4
* modify
  * merge rasa to Gameplay Scene
* add
  * Inspector ui

> 8/11

* modify
  * package -- HandLandmarkListAnnotation, ListAnnotation, HolisticLandmarkListAnnotation
* add
  * finish Inspector ui
* TODO: hint ui

> 8/12

* add
  * Acupuncture Item Prefab -- UITitle
  * hint panel ui
* TODO: switch between hint ui and Acupunture Item Prefab UITitle

> 8/12

* add 
  * finish hint panel ui
*  modify
  * UIInventoryItem -- change its behavior to favor user action
* TODO: Build Exercise Scene

> 8/12

* modify
  * fix hint panel problem -- panel not disabled
  * some fix for building
* add
  * Exercise Scene -- Setup

> 8/17

* modify
  * DatatBase Update
* add
  * dialogue system
