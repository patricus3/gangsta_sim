<?php
function defeat($by, array &$events, array &$state)
{
    $lost = intdiv($state['cash'], 2);
    $state['cash'] -= $lost;
    if($by!="cops")
    {
    $events[]="$by defeated you, they hit you in the head, knocking you unconscious, the fucking cops arrive, taking you to the jail, you bail out paying \$$lost ";
    }
    else{
    $events[] = "You were defeated by the fucking $by! They cuff you and  You're arrested but you bail out paying \$$lost! fuck!";
}
}