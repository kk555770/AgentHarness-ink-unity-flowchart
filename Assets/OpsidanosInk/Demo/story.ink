-> start

=== start ===
你好！這是一個最小範例。 # speaker:旁白 # cg:clear
我會示範幾個 Tag，包含新的 `char` JSON。 # speaker:旁白 # bg:room_01 # bgm:opening
* [角色狀態測試（char JSON）] -> char_test
* [往左走（舊範例）] -> left
* [往右走（舊範例）] -> right
* [畫面抖動測試（shake）] -> shake_test

=== char_test ===
第一句：alice 在中間（normal），bs 在左邊。 # speaker:旁白 # char:\{\"left\":\{\"actor\":\"bs\"\},\"center\":\{\"actor\":\"alice\",\"expr\":\"normal\"\}\}
第二句：ss 出現與 alice 移動同時發生（appear=0.5）。 # speaker:旁白 # char:\{\"transition\":\{\"appear\":0.5,\"steps\":[\{\"actions\":[\"appear\",\"move\"]\}]\},\"left\":\{\"actor\":\"bs\"\},\"center\":\{\"actor\":\"ss\"\},\"right\":\{\"actor\":\"alice\",\"expr\":\"normal\"\}\}
第三句：把 alice 移回中間並換表情（move=1）。 # speaker:旁白 # char:\{\"transition\":\{\"move\":1\},\"left\":\{\"actor\":\"bs\"\},\"center\":\{\"actor\":\"alice\",\"expr\":\"happy\"\},\"right\":\{\"actor\":\"ss\"\}\}
第四句：bs 和 ss 交換位置（move=1），移動中的角色會暫時在最上面。 # speaker:旁白 # char:\{\"transition\":\{\"move\":1\},\"left\":\{\"actor\":\"ss\"\},\"center\":\{\"actor\":\"alice\",\"expr\":\"happy\"\},\"right\":\{\"actor\":\"bs\"\}\}
第五句：先把 alice 置頂（無動作，raiseActors），再讓 bs/ss 移動（move=1），且移動這一步不自動置頂（raise=false）。 # speaker:旁白 # char:\{\"transition\":\{\"move\":1,\"steps\":[\{\"raiseActors\":[\"alice\"]\},\{\"actions\":[\"move\"],\"raise\":false\}]\},\"left\":\{\"actor\":\"bs\"\},\"center\":\{\"actor\":\"alice\",\"expr\":\"happy\"\},\"right\":\{\"actor\":\"ss\"\}\}
第六句：先讓 alice 消失（disappear=0.5），再讓 ss 移動到中間（move=0.7）。 # speaker:旁白 # char:\{\"transition\":\{\"move\":0.7,\"disappear\":0.5,\"steps\":[\{\"actions\":[\"disappear\"]\},\{\"actions\":[\"move\"]\}]\},\"left\":\{\"actor\":\"bs\"\},\"center\":\{\"actor\":\"ss\"\}\}
第七句：bs 瞬間移動到右邊（move=0），同時 ss 消失（disappear=0.5）。 # speaker:旁白 # char:\{\"transition\":\{\"move\":0,\"disappear\":0.5,\"steps\":[\{\"actions\":[\"move\",\"disappear\"]\}]\},\"right\":\{\"actor\":\"bs\"\}\}
第八句：全部清空。 # speaker:旁白 # char:clear
-> END

=== shake_test ===
第一下：抖一下。 # speaker:旁白 # shake
第二下：再抖一下。 # speaker:旁白 # shake
第三下：結束。 # speaker:旁白
-> END

=== left ===
你往左走，看到一個箱子。 # speaker:旁白 # char-left:alice_happy
你打開箱子，裡面是空的。 # speaker:旁白 # se:open # cg:alice_happy_cg
你揉揉眼睛，畫面又恢復了。 # speaker:旁白 # cg:clear
-> END

=== right ===
你往右走，看到一扇門。 # speaker:旁白 # char-right:alice_shy
你推開門，外面很亮。 # speaker:旁白 # bg:outside # shake
-> END
