interaction-verb-invalid = Некоторые требования для этого глагола не выполнены. Вы не можете его использовать прямо сейчас.
interaction-verb-cooldown = Этот глагол на перезарядке. Подождите {TOSTRING($seconds, "F1")} секунд.
interaction-HelpUp-name = Помочь встать 
interaction-HelpUp-description = Помоги существу встать.  
interaction-HelpUp-delayed-self-popup = Вы пытаетесь помочь {THE($target)} встать...  
interaction-HelpUp-delayed-target-popup = {THE($user)} пытается помочь вам встать...  
interaction-HelpUp-success-self-popup = Вы помогаете {THE($target)} встать.  
interaction-HelpUp-success-target-popup = {THE($user)} помогает вам встать.  
interaction-HelpUp-success-others-popup = {THE($user)} помогает {THE($target)} встать.  
interaction-HelpUp-fail-self-popup = Вам не удается помочь {THE($target)} встать.  
interaction-HelpUp-fail-target-popup = {THE($user)} не удается помочь вам встать.  
interaction-ForceDown-name = Уложить на пол
interaction-ForceDown-description = Принудите человека лечь на пол.  
interaction-ForceDown-delayed-self-popup = Вы пытаетесь принудить {THE($target)} лечь...  
interaction-ForceDown-delayed-target-popup = {THE($user)} пытается принудить вас лечь...  
interaction-ForceDown-success-self-popup = Вы принуждаете {THE($target)} лечь.  
interaction-ForceDown-success-target-popup = {THE($user)} принуждает вас лечь.  
interaction-ForceDown-success-others-popup = {THE($user)} принуждает {THE($target)} лечь.  
interaction-ForceDown-fail-self-popup = Вам не удается принудить {THE($target)} лечь.  
interaction-ForceDown-fail-target-popup = {THE($user)} не удается принудить вас лечь.  
interaction-MakeSleepOther-name = Уложить спать
interaction-MakeSleepOther-description = Уложить цель спать.  
interaction-MakeSleepOther-delayed-self-popup = Вы пытаетесь уложить {THE($target)} спать...  
interaction-MakeSleepOther-delayed-target-popup = {THE($user)} пытается уложить вас спать...  
interaction-MakeSleepOther-delayed-others-popup = {THE($user)} пытается уложить {THE($target)} спать...  
interaction-MakeSleepOther-fail-self-popup = Вам не удается уложить {THE($target)} спать.  
interaction-MakeSleepOther-fail-target-popup = {THE($user)} не удается уложить вас спать.  
interaction-MakeSleepOther-success-self-popup = Вы укладываете {THE($target)} спать.  
interaction-MakeSleepOther-success-target-popup = {THE($user)} укладывает вас спать.  
interaction-MakeSleepOther-success-others-popup = {THE($user)} укладывает {THE($target)} спать.  
interaction-ShakeOther-name = Потрясти  
interaction-ShakeOther-description = Потрясти цель.  
interaction-ShakeOther-fail-self-popup = Вам не удается как-то потрясти {THE($target)}.  
interaction-ShakeOther-fail-target-popup = {THE($user)} как-то не удается потрясти вас.  
interaction-ShakeOther-success-self-popup = Вы берете и трясете {THE($target)}.  
interaction-ShakeOther-success-target-popup = {THE($user)} берет и трясет вас.  
interaction-ShakeOther-success-others-popup = {THE($user)} берет и трясет {THE($target)}.  
interaction-LookAt-name = Посмотреть  
interaction-LookAt-description = Уставьтесь в пустоту и увидьте, как она смотрит на вас в ответ.  
interaction-LookAt-success-self-popup = Вы смотрите на {THE($target)}.  
interaction-LookAt-success-target-popup = Вы чувствуете, как {THE($user)} смотрит на вас...  
interaction-LookAt-success-others-popup = {THE($user)} смотрит на {THE($target)}.  
interaction-Hug-name = Обнять  
interaction-Hug-description = Одно объятие в день, может вылечить психологическую травму.  
interaction-Hug-success-self-popup = Вы обнимаете {THE($target)}.  
interaction-Hug-success-target-popup = {THE($user)} обнимает вас.  
interaction-Hug-success-others-popup = {THE($user)} обнимает {THE($target)}.  
interaction-Pet-name = Погладить  
interaction-Pet-description = Погладьте вашего коллегу, чтобы облегчить его стресс.  
interaction-Pet-success-self-popup = Вы гладите {THE($target)} по {POSS-ADJ($target)} голове.  
interaction-Pet-success-target-popup = {THE($user)} гладит вас по {POSS-ADJ($target)} голове.  
interaction-Pet-success-others-popup = {THE($user)} гладит {THE($target)}.  
interaction-KnockOn-name = Постучать  
interaction-KnockOn-description = Постучите по цели, чтобы привлечь внимание.  
interaction-KnockOn-success-self-popup = Вы стучите по {THE($target)}.  
interaction-KnockOn-success-target-popup = {THE($user)} стучит по вам.  
interaction-KnockOn-success-others-popup = {THE($user)} стучит по {THE($target)}.  
interaction-Rattle-name = Потрясти  
interaction-Rattle-success-self-popup = Вы трясете {THE($target)}.  
interaction-Rattle-success-target-popup = {THE($user)} трясет вас.  
interaction-Rattle-success-others-popup = {THE($user)} трясет {THE($target)}.  
interaction-WaveAt-name = Помахать  
interaction-WaveAt-description = Помахать цели. Если вы держите предмет, вы поможете им.  
interaction-WaveAt-success-self-popup = Вы машете {$hasUsed ->
    [false] в сторону {THE($target)}.
    *[true] своим {$used} в сторону {THE($target)}.
}  
interaction-WaveAt-success-target-popup = {THE($user)} машет {$hasUsed ->
    [false] вам.
    *[true] {POSS-PRONOUN($user)} {$used} вам.
}  
interaction-WaveAt-success-others-popup = {THE($user)} машет {$hasUsed ->
    [false] в сторону {THE($target)}.
    *[true] {POSS-PRONOUN($user)} {$used} в сторону {THE($target)}.
}  
interaction-PinchSelf-name = Ущепить себя
interaction-PinchSelf-description = Говорят, что это помогает убедиться, что ад вокруг вас — не сон.
interaction-PinchSelf-success-self-popup = Вы щипаете себя... Ай!
interaction-PinchSelf-success-others-popup = {THE($user)} щипает {REFLEXIVE($user)}... Выглядит болезненно!
interaction-PinchSelf-fail-self-popup = Вам как-то не удается щипнуть себя. Так лучше для вас.  
interaction-PinchSelf-delayed-self-popup = Вы щипаете себя...
interaction-PinchSelf-message-1 = Ай!!
interaction-PinchSelf-message-2 = Аааа!!
interaction-PinchSelf-message-3 = Оу!!
interaction-MakeSleepSelf-name = Спать
interaction-MakeSleepSelf-description = Уложите себя спать.
interaction-MakeSleepSelf-delayed-self-popup = Вы пытаетесь заснуть...
interaction-MakeSleepSelf-fail-self-popup = Вы не можете сейчас спать.
interaction-MakeSleepSelf-success-self-popup = Вы укладываете себя спать.
interaction-MakeSleepSelf-success-others-popup = {THE($user)} засыпает.
verb-categories-interaction = Взаимодействовать