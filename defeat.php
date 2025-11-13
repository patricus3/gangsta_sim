<?php
function defeat($by, array &$events, array &$state)
{
    $lost = intdiv($state['cash'], 2);
    $state['cash'] -= $lost;
    $events[] = "You were defeated by the fucking $by! They cuff you and take \$$lost (50% of your cash). You're arrested briefly.";
}
