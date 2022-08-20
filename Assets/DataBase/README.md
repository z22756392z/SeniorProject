資料庫變數:



| var          | mean                                                         | type   |
| ------------ | ------------------------------------------------------------ | ------ |
| id           | 資料編號                                                     | int    |
| title        | 資料名稱                                                     | string |
| content      | 資料內容                                                     | string |
| disease_type | 可治療疾病類型，這個我覺得要再修改，因為太多種疾病了         | string |
| rel_position | 定位的mediapipe編號                                          | int    |
| offset_x     | x偏移量(-10~10，間距可以再做調整)                            | int    |
| offset_y     | y偏移量(-10~10)                                              | int    |
| customize    | 1: 正面, 2: 背面,3: 正面左邊(手), 4: 背面左邊(手), 5: 正面右邊(手), 6: 背面右邊(手), 7: 其他 | int    |

![](picture/mediapipeLandmark.png)



正面右手(5)

![](picture/hand01.png)

背面右手(6)

![](picture/hand02.png)



背面左手(4)? 資料庫裡面的資料左右手有些還需要確認，因為有完整左右手穴位的網站很少..

![](https://github.com/derek120432/SeniorProject/blob/main/DataBase/picture/hand03.png)

左手背面

![左手背面](https://github.com/derek120432/SeniorProject/blob/main/DataBase/picture/hand05.png)

左手正面

![左手正面](https://github.com/derek120432/SeniorProject/blob/main/DataBase/picture/hand04.png)
***
# 症狀一覽
```
【功效】現代：流行性感冒，急性咽喉炎、扁桃體炎、腮腺炎、白喉、百日咳、慢性支氣管炎（急性發作期）、鼻衄、昏厥、休克、胎位不正、精神分裂症。
發熱、自汗、嬰幼兒腹瀉、拇指屈肌肌腱炎。
感冒，面神經麻痹，面肌痙攣，三叉神經痛，齒神經痛，咽炎，吞嚥不利，神經衰弱，癔病，前臂神經痛，支氣管炎，哮喘，顳頜關節功能紊亂，鼻炎，
痛經，閉經，滯產，產婦宮縮無力，產後乳少，單純性甲狀腺腫，小兒消化不良，急性胰腺炎，結膜炎，電光性眼炎，關節痛。
心律失常
疲勞乏力、舌縱不收。
泄熱、聰耳
清熱、利竅、舒筋。
肋間神經痛、瘧疾、項強、落枕、急性腰扭傷。
牙痛，五噎反胃，吐食，吞酸，鼻血，食道痙攣，食慾減退，胃擴張，呃逆(打嗝)，
白癜風(白癜風（Vitiligo）也稱為白斑、白蝕、白斑症、白蝕症，是慢性的皮膚症狀，特徵是皮膚部份部位因為色素脫失而出現斑痕 。)。
乳汁分泌不足、乳腺炎、前臂痛、昏迷。
頭痛，目赤，耳鳴，耳聾，咽喉腫痛，手臂紅腫疼痛，以及肘間神經痛等
現代：膽囊炎、肘腕及指關節炎、糖尿病、胃炎。
尺神經痛、癲癇、腕三角軟骨盤損傷。
肩臂痛。
止痙、開竅、利咽。
頭風(反覆發作的頭痛)，手臂紅腫，手指麻木，頭項強痛(頭項部牽強不舒作痛)，咽痛，齒痛，目痛，煩熱，毒蛇咬傷。
口腔頜面病症、失眠。
舌肌麻痹。
口齒頜面病症
肺結核、胸痛、無脈症(是指主動脈及其主要分支的慢性進行性非特異的炎性疾病)、脈管炎、心悸。
心肌炎、腕管綜合征。
中暑、心痛。小兒夜啼、舌強腫痛。
蕁麻疹、腕背腱鞘囊腫。
橈骨莖突部狹窄性腱鞘炎(媽媽手)。
掌指麻痹，不能伸屈，消化不良。

```

穴位參考網站: 

http://cht.a-hospital.com/w/%E4%BA%BA%E4%BD%93%E7%A9%B4%E4%BD%8D%E5%9B%BE

https://www.acupoint361.com/2018/09/plam-acupoints.html

穴位資料:

https://yibian.hopto.org/db/?ano=429

手部mediapipe參考網站: https://google.github.io/mediapipe/solutions/hands.html
