# コマンドリスト


## 一般ユーザー向けのコマンド


| ConVar         | Description           | note                                                           |
|----------------|-----------------------|----------------------------------------------------------------|
| css_timeleft   | 残り時間を表示します            |                                                                |
| css_nextmap    | 次のマップを表示します           |                                                                |
| css_currentmap | 現在のマップを表示します          |                                                                |
| css_mapinfo    | マップ情報がある場合はその情報を表示します |                                                                |
| css_extends    | 残りの延長回数を表示します         |                                                                |
| css_revote     | 再投票をする際に使用します         |                                                                |
| css_nominate   | マップをノミネートできます         |                                                                |
| css_nomlist    | ノミネートされたマップを確認できます    | 管理者の人は引数に `full` をつけるとノミネートしたユーザの名前を確認できます。 例: `!nomlist full` |
| css_rtv        | Rock The Vote         |                                                                |

## 管理者向けのコマンド


| ConVar                 | Description           | note |
|------------------------|-----------------------|------|
| css_setnextmap         | 次のマップを設定します           |      |
| css_removenextmap      | 次のマップを削除します           |      |
| css_setmapcooldown     | 指定したマップのクールダウンを設定します  |      |
| css_setgroupcooldown   | 指定したグループのクールダウンを設定します |      |
| css_cancelvote         | 現在進行中の投票をキャンセルします     |      |
| css_nominate_addmap    | マップをノミネートリストに追加します    |      |
| css_nominate_removemap | マップをノミネートリストから削除します   |      |
| css_enablertv          | RTVを有効化               |      |
| css_disablertv         | RTVを無効化               |      |
| css_forcertv           | 強制的にRTVを使用できます        |      |