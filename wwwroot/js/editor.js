window.onload = checkAuthentication;

function checkAuthentication() {
    const cookies = document.cookie.split(";").reduce((prev, current) => {
        const [name, value] = current.split("=").map(c => c.trim());
        prev[name] = value;
        return prev;
    }, {});

    if (cookies.isAuthenticated === "true") {
        document.getElementById("passwordPopup").classList.add("hidden");
        document.getElementById("content").classList.remove("hidden");
    } else {
        document.getElementById("passwordPopup").classList.remove("hidden");
        document.getElementById("content").classList.add("hidden");
    }
}

// パスワード認証
async function submitPassword() {
    const password = document.getElementById("change-password").value;
    const formData = new FormData();
    formData.append('password', password);

    const response = await fetch('/api/ValidatePassword', {
        method: 'POST',
        body: formData
    });

    if (response.ok) {
        // Set authenticated state in a cookie
        document.cookie = "isAuthenticated=true; path=/";
        document.getElementById("passwordPopup").classList.add("hidden");
        document.getElementById("content").classList.remove("hidden");
        currentPassword = password;
    } else {
        alert("パスワードが違います");
    }
}

// パスワード変更
async function changePassword() {
    const password = document.getElementById("new-password").value;
    const formData = new FormData();
    formData.append('password', password);

    const response = await fetch('/api/ChangePassword', {
        method: 'POST',
        body: formData
    });

    if (response.ok) {
        currentPassword = password;
        document.getElementById("password-result").innertext = "パスワードを変更しました";
    } else {
        document.getElementById("password-result").innertext = "パスワード変更に失敗しました";
    }
}

// 概要ページ読み込み
async function readHTML() {
    const fileUrl = '/files/about.html';

    fetch(fileUrl)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.text();
        })
        .then(data => {
            const lines = data.split('\n');
            const headline = lines[0]; // 見出しを取得
            document.getElementById("headlineText").value = headline;
            const mainText = lines.slice(1).join('\n'); // 本文を取得
            document.getElementById("htmlSource").value = mainText;
        })
        .catch(error => {
            console.error('There has been a problem with your fetch operation:', error);
        });
}

// 概要ページ変更プレビュー
async function previewHTML() {
    document.getElementById("preview-box").style.display = "block";
    document.getElementById("html-preview").innerHTML = document.getElementById("headlineText").value + document.getElementById("htmlSource").value
}

// 概要ページ更新
async function editHTML() {
    if (confirm("更新しますか？")) {
        const headlineText = document.getElementById("headlineText").value;
        const htmlSource = document.getElementById("htmlSource").value;
        const formData = new FormData();
        formData.append('headlineText', headlineText);
        formData.append('htmlSource', htmlSource);

        const response = await fetch('/api/EditHTML', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            readHTML()
            alert("更新しました");
        } else {
            alert("更新に失敗しました");
        }
    }
}

// メディア一覧編集画面
document.addEventListener("DOMContentLoaded", function () {
    const applyButton = document.getElementById("apply-button");
    const backButton = document.getElementById("back-button");
    const confirmButton = document.getElementById("confirm-button");
    const resetAButton = document.getElementById("reset-a-button");
    const resetBButton = document.getElementById("reset-b-button");
    const updatedTable = document.getElementById("updated-table");

    let originalData = [];
    let initialData = []; // Variable to hold the initial state

    function getCurrentData() {
        const rows = document.querySelectorAll("#edit-table tr");
        const data = [];

        for (let i = 1; i < rows.length; i++) { // Skip header
            const cells = rows[i].querySelectorAll("td");
            const title = cells[1].querySelector("input").value;
            const priority = cells[2].querySelector(".prioritySelector").value;
            const isShow = cells[4].querySelector(".isShowSelector").value == "1";
            const name = cells[5].innerText;

            data.push({
                idx: i,
                index: cells[0].innerText,
                title: title,
                priority: parseInt(priority, 10),
                count: cells[3].innerText,
                isShow: isShow,
                name: name
            });
        }

        return data;
    }

    function highlightChanges(newData) {
        // Compare data and highlight changes
        newData.forEach((item, index) => {
            const originalItem = originalData.find(orig => orig.name === item.name);
            const row = document.querySelector(`#edit-table tr:nth-child(${item.idx + 1})`);
            const priorityCell = row.querySelector('td:nth-child(3)');
            const titleCell = row.querySelector('td:nth-child(2)');

            if (originalItem) {
                if (item.priority !== originalItem.priority) {
                    priorityCell.classList.add('highlight');
                    item.priority_changed = true; // mark for highlighting in the sorted table
                } else {
                    priorityCell.classList.remove('highlight');
                    item.priority_changed = false; // remove mark if not changed
                }

                if (item.isShow !== originalItem.isShow) {
                    row.classList.add('highlight');
                    item.isShow_changed = true; // mark for highlighting in the sorted table
                } else {
                    row.classList.remove('highlight');
                    item.isShow_changed = false; // remove mark if not changed
                }

                if (item.title !== originalItem.title) {
                    titleCell.querySelector('input').classList.add('highlight');
                    item.title_changed = true; // Set the title_changed flag if title has changed
                } else {
                    titleCell.querySelector('input').classList.remove('highlight');
                    item.title_changed = false; // Remove the title_changed flag if title has not changed
                }
            }
        });
    }

    function updateTable() {
        const newData = getCurrentData();

        // Highlight changes before sorting
        highlightChanges(newData);

        // Sort the new data after highlighting
        newData.sort((a, b) => {
            if (a.isShow !== b.isShow) {
                return b.isShow - a.isShow;
            }
            return a.priority - b.priority;
        });

        // Update priority values to be sequential
        let visiblePriority = 0;
        newData.forEach((item, index) => {
            item.priority = visiblePriority++;
        });

        // Clear the updated table
        while (updatedTable.firstChild) {
            updatedTable.removeChild(updatedTable.firstChild);
        }

        const header = document.createElement('tr');
        header.innerHTML = "<th>連番</th><th>動画名</th><th>表示順</th><th>再生回数</th><th>表示/非表示</th>";
        updatedTable.appendChild(header);

        newData.forEach(function (item, index) {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${item.index}</td>
                <td ${item.title_changed ? 'class="highlight"' : ''}>${item.title}</td>
                <td ${item.priority_changed ? 'class="highlight"' : ''}>${item.priority}</td>
                <td>${item.count}</td>
                <td ${item.isShow_changed ? 'class="highlight"' : ''}>${item.isShow ? '表示' : '非表示'}</td>
            `;

            if (item.isShow_changed) {
                row.classList.add('highlight');
            }

            updatedTable.appendChild(row);
        });

        document.getElementById("menu-item-main").style.display = "none";
        document.getElementById("modified").style.display = "block";
    }

    function resume() {
        document.getElementById("modified").style.display = "none";
        var menuItemMain = document.getElementById("menu-item-main");
        menuItemMain.style.display = "block";
        var textboxes = menuItemMain.querySelectorAll('input[type="text"]');
        textboxes.forEach(function (textbox) {
            textbox.style.width = "500px";
        });

        // Re-apply highlights based on current edit-table state
        highlightChanges(getCurrentData());
    }

    function resetToInitial() {
        originalData = JSON.parse(JSON.stringify(initialData)); // Restore the original data
        renderEditTable(originalData);
        applyChanges();
    }

    function applyChanges() {
        const rows = document.querySelectorAll("#edit-table tr");
        const updatedData = [];

        for (let i = 1; i < rows.length; i++) { // Skip header
            const name = rows[i].querySelector(".nameSelector").innerText;
            const title = rows[i].querySelector("input").value;
            const priority = rows[i].querySelector(".prioritySelector").value;
            const isShow = rows[i].querySelector(".isShowSelector").value == "1";

            updatedData.push({
                Name: name,
                Title: title,
                Priority: parseInt(priority, 10),
                IsShow: isShow
            });
        }

        // API へデータを送信
        fetch('/api/UpdateMediaRecord', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(updatedData)
        }).then(response => response.json())
            .then(data => {
                console.log('Success:', data);

                // Update originalData with the new values
                originalData = updatedData;  // ここでoriginalDataを更新

                // Render the edit table again with updatedData
                renderEditTable(originalData);

                // Return to edit view
                resume();
            }).catch((error) => {
                console.error('Error:', error);
                // Ensure to return to edit view even on error (optional)
                resume();
            });
    }

    function renderEditTable(data) {
        const table = document.getElementById("edit-table");
        const rows = Array.from(table.querySelectorAll("tr"));

        // Clear all rows except the header
        for (let i = rows.length - 1; i > 0; i--) {
            table.deleteRow(i);
        }

        // Sort data by priority, then by isShow 
        data.sort((a, b) => {
            if (a.isShow !== b.isShow) {
                return b.isShow - a.isShow;
            }
            return a.priority - b.priority;
        });

        // Add new rows
        data.forEach((item, i) => {
            const row = table.insertRow();
            row.innerHTML = `
                <td>${item.index}</td>
                <td><input type="text" value="${item.title}"></td>
                <td>
                    <select class="prioritySelector" name="priority_${i}">
                        <!-- Options will be added dynamically in the next step -->
                    </select>
                </td>
                <td>${item.count}</td>
                <td>
                    <select class="isShowSelector" name="isShow_${i}">
                        <option value="1" ${item.isShow ? 'selected' : ''}>表示</option>
                        <option value="0" ${!item.isShow ? 'selected' : ''}>非表示</option>
                    </select>
                </td>
                <td style="display:none" class="nameSelector" name="${item.name}">${item.name}</td>
            `;

            const prioritySelect = row.querySelector(".prioritySelector");

            // Add priority options based on the length of data
            for (let j = 0; j < data.length; j++) {
                prioritySelect.options.add(new Option(j, j, false, item.priority === j));
            }
        });

        addEventListenersForHighlight(); // Re-add event listeners for new elements
        highlightChanges(data);
    }

    function addEventListenersForHighlight() {
        const prioritySelectors = document.querySelectorAll(".prioritySelector");
        const isShowSelectors = document.querySelectorAll(".isShowSelector");
        const titleInputs = document.querySelectorAll("input[type='text']");

        prioritySelectors.forEach((selector, index) => {
            selector.addEventListener("change", () => {
                const currentData = getCurrentData();
                const row = selector.closest('tr');
                const originalItem = originalData.find(item => item.idx === currentData[index].idx);

                if (currentData[index].priority !== originalItem.priority) {
                    selector.closest('td').classList.add('highlight');
                } else {
                    selector.closest('td').classList.remove('highlight');
                }
            });
        });

        isShowSelectors.forEach((selector, index) => {
            selector.addEventListener("change", () => {
                const currentData = getCurrentData();
                const row = selector.closest('tr');
                const originalItem = originalData.find(item => item.idx === currentData[index].idx);

                if (currentData[index].isShow !== originalItem.isShow) {
                    row.classList.add('highlight');
                } else {
                    row.classList.remove('highlight');
                }
            });
        });

        titleInputs.forEach((input, index) => {
            input.addEventListener("input", () => {
                const currentData = getCurrentData();
                const titleCell = input.closest('td');
                const originalItem = originalData.find(item => item.idx === currentData[index].idx);

                if (currentData[index].title !== originalItem.title) {
                    titleCell.classList.add('highlight');
                } else {
                    titleCell.classList.remove('highlight');
                }
            });
        });
    }

    applyButton.addEventListener("click", applyChanges);
    backButton.addEventListener("click", resume);
    confirmButton.addEventListener("click", updateTable);
    resetAButton.addEventListener("click", resetToInitial);
    resetBButton.addEventListener("click", resetToInitial);

    // Store the original data when the page is loaded
    window.addEventListener("load", () => {
        originalData = getCurrentData();
        initialData = JSON.parse(JSON.stringify(originalData)); // Deep copy of originalData
        currentPassword = "";
        addEventListenersForHighlight();
    });

    // 動画アップロード
    document.getElementById('uploadForm').addEventListener('submit', async function (event) {
        event.preventDefault();

        const baseFileName = document.getElementById('baseFileName').textContent.trim();
        if (!baseFileName) {
            alert("動画・サムネイル画像に設定する共通のファイル名を入力してください");
            return;
        }

        const videoFile = document.getElementById('videoFile').files[0];
        const thumbFile = document.getElementById('thumbFile').files[0];

        if (!videoFile || !thumbFile) {
            alert("動画ファイルとサムネイル画像の両方を指定してください");
            return;
        }

        const formData = new FormData();
        formData.append('baseFileName', baseFileName);
        formData.append('videoFile', videoFile);
        formData.append('thumbFile', thumbFile);

        const xhr = new XMLHttpRequest();
        xhr.open('POST', '/api/Upload', true);

        // プログレスバー表示
        xhr.upload.addEventListener('progress', function (event) {
            if (event.lengthComputable) {
                const percentComplete = (event.loaded / event.total) * 100;
                const progressBar = document.getElementById('progressBar').firstElementChild;
                progressBar.style.width = percentComplete + '%';
                progressBar.textContent = Math.round(percentComplete) + '%';
            }
        });

        xhr.onload = function () {
            if (xhr.status === 200) {
                alert("アップロードが完了しました");
                const progressBar = document.getElementById('progressBar').firstElementChild;
                progressBar.style.width = '0%';
                progressBar.textContent = '';
            } else {
                alert("アップロードに失敗しました");
            }
        };

        xhr.send(formData);
    });
});
