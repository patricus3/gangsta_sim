<?php
function pkdf2_encrypt(string $payload, string $pass, int $iterations = 100000): ?string
{
    $salt = random_bytes(16);
    $key = hash_pbkdf2('sha256', $pass, $salt, $iterations, 32, true);
    $ivlen = openssl_cipher_iv_length('AES-256-CBC');
    $iv = random_bytes($ivlen);

    $cipher = openssl_encrypt($payload, 'AES-256-CBC', $key, OPENSSL_RAW_DATA, $iv);
    if ($cipher === false) return null;
    // Return salt + IV + ciphertext, base64-encoded
    return base64_encode($salt . $iv . $cipher);
}

function pkdf2_decrypt(string $encoded, string $pass, int $iterations = 100000): ?string
{
    $data = base64_decode($encoded, true);
    if ($data === false) return null;

    $salt = substr($data, 0, 16);
    $ivlen = openssl_cipher_iv_length('AES-256-CBC');
    $iv = substr($data, 16, $ivlen);
    $cipher = substr($data, 16 + $ivlen);

    $key = hash_pbkdf2('sha256', $pass, $salt, $iterations, 32, true);
    return openssl_decrypt($cipher, 'AES-256-CBC', $key, OPENSSL_RAW_DATA, $iv);
}
?>