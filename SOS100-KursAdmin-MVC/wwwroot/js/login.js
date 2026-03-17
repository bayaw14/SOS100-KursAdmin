function switchTab(role) {
    document.getElementById('tab-student').classList.toggle('active', role === 'student');
    document.getElementById('tab-teacher').classList.toggle('active', role !== 'student');
}

document.querySelector('form').addEventListener('submit', function () {
    var btn = document.getElementById('login-btn');
    btn.disabled = true;
    document.getElementById('btn-label').textContent     = 'Loggar in…';
    document.getElementById('btn-spinner').style.display = 'block';
    document.getElementById('btn-arrow').style.display   = 'none';
});
