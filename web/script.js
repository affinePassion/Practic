var currentUser = {
    id : null,
    name : null,
    role : null,
    position : null
}

var currentObjects = [];


//REGISTER
document.getElementById('register-button').addEventListener('click', function() {
    // Собираем данные регистрации
    const email = document.getElementById('register-email').value;
    const password = document.getElementById('register-password').value;
    const confirmPassword = document.getElementById('register-confirm-password').value;
    const fullName = document.getElementById('full-name').value;
    const position = document.getElementById('position').value;
    const birth_date = document.getElementById('birth-date').value;
    const role = document.getElementById('role').value;

    if (password === confirmPassword) {
        fetch('http://localhost:8080/api/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password, fullName, position, birth_date, role })
        })
        .then(response => response.json())
        .then(data => {
            if (data && data.id && data.name && data.role) {
                alert('Регистрация прошла успешно!');
                const currentUser = {
                    id: data.id,
                    name: data.name,
                    role: data.role
                };
                showPanel(currentUser.role, email);
            } else {
                alert('Ошибка регистрации!');
            }
        })
        .catch(error => {
            console.error('Ошибка:', error);
        });
    } else {
        alert('Пароли не совпадают!');
    }
});


//LOGIN
document.getElementById('login-button').addEventListener('click', function() {
    // Собираем данные входа
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;

    fetch('http://localhost:8080/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, password })
    })
    .then(response => response.json())
    .then(data => {
        alert('Вы вошли как ' + data.role);
        currentUser.name = data.fio;
        currentUser.role = data.role;
        currentUser.id = data.id;
        currentUser.position = data.position;
        showPanel(data.role, email, data.position);
    })
    .catch(error => {
        console.error('Ошибка:', error);
    });
});

function showPanel(role, email, position) {

    // Скрываем формы регистрации и входа
    document.getElementById('login-form').style.display = 'none';
    document.getElementById('register-form').style.display = 'none';


    if (role == 'worker') {
        // Создаем контейнер для формы работника
        const workerContainer = document.createElement('div');
        workerContainer.className = 'worker-container';
    
        // Создаем иконку работника
        const workerIcon = document.createElement('img');
        workerIcon.src = 'worker-icon.png';
        workerIcon.className = 'worker-icon';
    
        // Создаем контейнер для информации о работнике
        const workerInfo = document.createElement('div');
        workerInfo.className = 'worker-info';
    
        // Создаем ФИО работника
        const workerFullName = document.createElement('h2');
        workerFullName.textContent = currentUser.name;
    
        // Создаем должность работника
        const workerPosition = document.createElement('p');
        workerPosition.textContent = `Должность: ${currentUser.position}`;
    
        // Добавляем ФИО и должность в контейнер информации о работнике
        workerInfo.appendChild(workerFullName);
        workerInfo.appendChild(workerPosition);
    
        // Создаем кнопку выхода
        const logoutButton = document.createElement('button');
        logoutButton.textContent = 'Выход';
        logoutButton.className = 'logout-button';
        logoutButton.addEventListener('click', () => {
            // Возвращаем формы регистрации и входа
            document.getElementById('login-form').style.display = 'block';
            document.getElementById('register-form').style.display = 'block';
            workerContainer.remove();
        });
    
        // Создаем контейнер для объектов
        const objectsContainer = document.createElement('div');
        objectsContainer.className = 'objects-container';
    
        // Создаем надпись "Объекты"
        const objectsHeader = document.createElement('h1');
        objectsHeader.textContent = 'Объекты';
    
        // Создаем объекты
        const objects = [];
    
        // Запрос
        fetch('http://localhost:8080/objects', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email })
        })
        .then(response => response.json())
        .then(data => {
            data.forEach(item => {
                objects.push({ id: item.id, name: item.name, status: item.status, address: item.address, enddate: item.enddate });
            });
            objects.forEach((object) => {
                const objectContainer = document.createElement('div');
                objectContainer.className = 'object-container';
    
                const objectName = document.createElement('button');
                objectName.textContent = object.name;
    
                // Добавляем обработчик события для кнопки объекта
                objectName.addEventListener('click', () => {
                    showObjectDetails(object);
                });
    
                const objectStatus = document.createElement('p');
                objectStatus.textContent = object.status;
    
                const objectAddress = document.createElement('p');
                objectAddress.textContent = object.address;
    
                objectContainer.appendChild(objectName);
                objectContainer.appendChild(objectStatus);
                objectContainer.appendChild(objectAddress);
    
                objectsContainer.appendChild(objectContainer);
            });
        })
        .catch(error => console.error('Ошибка:', error));
    
        // Добавляем элементы в контейнер работника
        workerContainer.appendChild(workerIcon);
        workerContainer.appendChild(workerInfo);
        workerContainer.appendChild(logoutButton);
        workerContainer.appendChild(objectsHeader);
        workerContainer.appendChild(objectsContainer);
    
        // Добавляем контейнер работника в страницу
        document.body.appendChild(workerContainer);
    
        function showObjectDetails(object) {
            // Скрываем все текущие элементы интерфейса
            workerContainer.style.display = 'none';
    
            // Создаем контейнер для информации об объекте
            const objectDetailsContainer = document.createElement('div');
            objectDetailsContainer.className = 'object-details-container';
    
            // Создаем элементы для отображения информации об объекте
            const objectTitle = document.createElement('h1');
            objectTitle.textContent = object.name;
    
            const objectAddress = document.createElement('p');
            objectAddress.textContent = `Адрес: ${object.address}`;
    
            const objectEnddate = document.createElement('p');
            objectEnddate.textContent = `Срок сдачи: ${object.enddate}`;
    
            const objectStatus = document.createElement('p');
            objectStatus.textContent = `Статус: ${object.status}`;
    
            // Добавляем элементы в контейнер информации об объекте
            objectDetailsContainer.appendChild(objectTitle);
            objectDetailsContainer.appendChild(objectAddress);
            objectDetailsContainer.appendChild(objectEnddate);
            objectDetailsContainer.appendChild(objectStatus);
    
            // Создаем контейнер для задач объекта
            const tasksContainer = document.createElement('div');
            tasksContainer.className = 'tasks-container';
    
            // Запрос задач объекта
            fetch('http://localhost:8080/tasks', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ objectId: object.id })
            })
            .then(response => response.json())
            .then(tasks => {
                tasks.forEach(task => {

                    const taskContainer = document.createElement('div');
                    taskContainer.className = 'task-container';
    
                    const taskLeft = document.createElement('div');
                    taskLeft.className = 'task-left';
    
                    const taskName = document.createElement('p');
                    taskName.textContent = task.name;
    
                    const taskDescription = document.createElement('p');
                    taskDescription.textContent = task.description;
    
                    taskLeft.appendChild(taskName);
                    taskLeft.appendChild(taskDescription);
    
                    const taskRight = document.createElement('div');
                    taskRight.className = 'task-right';
    
                    // Создаем комбо-бокс для статуса задачи
                    const statusSelect = document.createElement('select');
                    statusSelect.innerHTML = `
                        <option value="В ожидании">В ожидании</option>
                        <option value="В процессе">В процессе</option>
                        <option value="Выполнено">Выполнено</option>
                    `;
                    statusSelect.value = task.status;
    
                    // Обработчик изменения выбора статуса
                    statusSelect.addEventListener('change', () => {
                        // Выполняем PUT-запрос для обновления статуса задачи
                        fetch(`http://localhost:8080/tasks-update`, {
                            method: 'PUT',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({ id : task.id, status: statusSelect.value })
                        })
                        .then(response => {
                            if (!response.ok) {
                                throw new Error('Ошибка при обновлении статуса задачи');
                            }
                            return response.json();
                        })
                        .then(updatedTask => {
                            // Обновляем текст статуса задачи на странице
                            taskStatus.textContent = `Статус: ${updatedTask.status}`;
                        })
                        .catch(error => console.error('Ошибка:', error));
                    });
    
                    taskRight.appendChild(statusSelect);
    
                    taskContainer.appendChild(taskLeft);
                    taskContainer.appendChild(taskRight);
    
                    tasksContainer.appendChild(taskContainer);
                });
            })
            .catch(error => console.error('Ошибка:', error));
    
            // Создаем кнопку выхода
            const backButton = document.createElement('button');
            backButton.textContent = 'Назад';
            backButton.className = 'back-button';
            backButton.addEventListener('click', () => {
                objectDetailsContainer.remove();
                tasksContainer.remove();
                backButton.remove();
                workerContainer.style.display = 'flex';
            });
    
            // Добавляем элементы на страницу
            document.body.appendChild(objectDetailsContainer);
            document.body.appendChild(tasksContainer);
            document.body.appendChild(backButton);
        }
    }    
    
    if (role === 'manager') {
        // Создаем контейнер для формы работника
        const managerContainer = document.createElement('div');
        managerContainer.className = 'manager-container';
    
        // Создаем иконку работника
        const managerIcon = document.createElement('img');
        managerIcon.src = 'manager-icon.png';
        managerIcon.className = 'manager-icon';
    
        // Создаем контейнер для информации о работнике
        const managerInfo = document.createElement('div');
        managerInfo.className = 'manager-info';
    
        // Создаем ФИО работника
        const managerFullName = document.createElement('h2');
        managerFullName.textContent = currentUser.name;
    
        // Создаем должность работника
        const managerPosition = document.createElement('p');
        managerPosition.textContent = `Должность: ${currentUser.position}`;
    
        // Добавляем ФИО и должность в контейнер информации о работнике
        managerInfo.appendChild(managerFullName);
        managerInfo.appendChild(managerPosition);
    
        // Создаем кнопку выхода
        const logoutButton = document.createElement('button');
        logoutButton.textContent = 'Выход';
        logoutButton.className = 'logout-button';
        logoutButton.addEventListener('click', () => {
            // Возвращаем формы регистрации и входа
            document.getElementById('login-form').style.display = 'block';
            document.getElementById('register-form').style.display = 'block';
            managerContainer.remove();
        });
    
        // Создаем контейнер для секций
        const sectionsContainer = document.createElement('div');
        sectionsContainer.className = 'sections-container';
    
        // Создаем контейнеры для материалов, поставок, объектов и работников
        const materialsContainer = document.createElement('div');
        materialsContainer.className = 'section';
        const suppliesContainer = document.createElement('div');
        suppliesContainer.className = 'section';
        const objectsContainer = document.createElement('div');
        objectsContainer.className = 'section';
        const workersContainer = document.createElement('div');
        workersContainer.className = 'section';
    
        // Создаем надписи для каждого раздела
        const materialsHeader = document.createElement('h1');
        materialsHeader.textContent = 'Материалы на складе';
        const suppliesHeader = document.createElement('h1');
        suppliesHeader.textContent = 'Поставки';
        const objectsHeader = document.createElement('h1');
        objectsHeader.textContent = 'Объекты';
        const workersHeader = document.createElement('h1');
        workersHeader.textContent = 'Работники';
    
        // Создаем неактивные прямоугольники с текстом "В разработке"
        const viewMaterialsButton = document.createElement('button');
        viewMaterialsButton.className = 'view-materials-button';
        viewMaterialsButton.textContent = 'Посмотреть список материалов';
        viewMaterialsButton.addEventListener('click', () => {
            fetch('http://localhost:8080/materials', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => response.json())
            .then(data => {
                // Скрываем все текущие элементы интерфейса
                managerContainer.style.display = 'none';
                sectionsContainer.style.display = 'none';
    
                // Создаем контейнер для таблицы с рабочими
                const materialTableContainer = document.createElement('div');
                materialTableContainer.className = 'worker-table-container';
    
                // Создаем заголовок "Рабочие"
                const materialTitle = document.createElement('h1');
                materialTitle.textContent = 'Материалы на складе';
    
                // Создаем таблицу
                const materialTable = document.createElement('table');
                materialTable.className = 'worker-table';
    
                // Создаем заголовок таблицы
                const tableHeader = document.createElement('tr');
                const nameHeader = document.createElement('th');
                nameHeader.textContent = 'Наименование';
                const quantityHeader = document.createElement('th');
                quantityHeader.textContent = 'Количество';
                const unitHeader = document.createElement('th');
                unitHeader.textContent = 'Единица измерения(шт, кг)';
                const datedeliveryHeader = document.createElement('th');
                datedeliveryHeader.textContent = 'Дата поставки';
    
                tableHeader.appendChild(nameHeader);
                tableHeader.appendChild(quantityHeader);
                tableHeader.appendChild(unitHeader);
                tableHeader.appendChild(datedeliveryHeader);
                materialTable.appendChild(tableHeader);
    
                // Добавляем строки с данными рабочих
                data.forEach(material => {
                    const tableRow = document.createElement('tr');
                    const nameCell = document.createElement('td');
                    nameCell.textContent = material.name;
                    const quantityCell = document.createElement('td');
                    quantityCell.textContent = material.quantity;
                    const unitCell = document.createElement('td');
                    unitCell.textContent = material.unit;
                    const datedeliveryCell = document.createElement('td');
                    datedeliveryCell.textContent = material.deliverydate;
    
                    tableRow.appendChild(nameCell);
                    tableRow.appendChild(quantityCell);
                    tableRow.appendChild(unitCell);
                    tableRow.appendChild(datedeliveryCell);
                    materialTable.appendChild(tableRow);
                });
    
                // Создаем кнопку "Проверено"
                const backButton = document.createElement('button');
                backButton.textContent = 'Проверено';
                backButton.className = 'back-button';
                backButton.addEventListener('click', () => {
                    materialTableContainer.remove();
                    managerContainer.style.display = 'flex';
                    sectionsContainer.style.display = 'flex';
                });
    
                // Добавляем таблицу, заголовок и кнопку в контейнер
                materialTableContainer.appendChild(materialTitle);
                materialTableContainer.appendChild(materialTable);
                materialTableContainer.appendChild(backButton);
    
                // Добавляем контейнер с таблицей в body
                document.body.appendChild(materialTableContainer);
                materialTableContainer.style.display = 'flex';
                materialTableContainer.style.flexDirection = 'column';
                materialTableContainer.style.alignItems = 'center';
            })
            .catch(error => console.error('Ошибка:', error));
        });
    
        const viewDeliveriesButton = document.createElement('button');
        viewDeliveriesButton.className = 'view-deliveries-button';
        viewDeliveriesButton.textContent = 'Посмотреть список поставок';
        viewDeliveriesButton.addEventListener('click', () => {
            fetch('http://localhost:8080/materialdeliveries', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => response.json())
            .then(data => {
                // Скрываем все текущие элементы интерфейса
                managerContainer.style.display = 'none';
                sectionsContainer.style.display = 'none';
    
                // Создаем контейнер для таблицы с рабочими
                const deliveryTableContainer = document.createElement('div');
                deliveryTableContainer.className = 'worker-table-container';
    
                // Создаем заголовок "Рабочие"
                const deliveryTitle = document.createElement('h1');
                deliveryTitle.textContent = 'Поставки';
    
                // Создаем таблицу
                const deliveryTable = document.createElement('table');
                deliveryTable.className = 'worker-table';
    
                // Создаем заголовок таблицы
                const tableHeader = document.createElement('tr');
                const materialidHeader = document.createElement('th');
                materialidHeader.textContent = 'ID материала';
                const deliverydateHeader = document.createElement('th');
                deliverydateHeader.textContent = 'Дата поставки';
                const quantityHeader = document.createElement('th');
                quantityHeader.textContent = 'Количество';
                const unitHeader = document.createElement('th');
                unitHeader.textContent = 'Единица измерения(шт, кг)';
    
                tableHeader.appendChild(materialidHeader);
                tableHeader.appendChild(quantityHeader);
                tableHeader.appendChild(unitHeader);
                tableHeader.appendChild(deliverydateHeader);
                deliveryTable.appendChild(tableHeader);
    
                // Добавляем строки с данными рабочих
                data.forEach(delivery => {
                    const tableRow = document.createElement('tr');
                    const materialIDCell = document.createElement('td');
                    materialIDCell.textContent = delivery.materialid;
                    const deliverydateCell = document.createElement('td');
                    deliverydateCell.textContent = delivery.deliverydate;
                    const quantityCell = document.createElement('td');
                    quantityCell.textContent = delivery.quantity;
                    const unitCell = document.createElement('td');
                    unitCell.textContent = delivery.unit;
    
                    tableRow.appendChild(materialIDCell);
                    tableRow.appendChild(quantityCell);
                    tableRow.appendChild(unitCell);
                    tableRow.appendChild(deliverydateCell);
                    deliveryTable.appendChild(tableRow);
                });
    
                // Создаем кнопку "Проверено"
                const backButton = document.createElement('button');
                backButton.textContent = 'Проверено';
                backButton.className = 'back-button';
                backButton.addEventListener('click', () => {
                    deliveryTableContainer.remove();
                    managerContainer.style.display = 'flex';
                    sectionsContainer.style.display = 'flex';
                });
    
                // Добавляем таблицу, заголовок и кнопку в контейнер
                deliveryTableContainer.appendChild(deliveryTitle);
                deliveryTableContainer.appendChild(deliveryTable);
                deliveryTableContainer.appendChild(backButton);
    
                // Добавляем контейнер с таблицей в body
                document.body.appendChild(deliveryTableContainer);
                deliveryTableContainer.style.display = 'flex';
                deliveryTableContainer.style.flexDirection = 'column';
                deliveryTableContainer.style.alignItems = 'center';
            })
            .catch(error => console.error('Ошибка:', error));
        });
    
        // Создаем кнопку "Посмотреть список рабочих"
        const viewWorkersButton = document.createElement('button');
        viewWorkersButton.textContent = 'Посмотреть список рабочих';
        viewWorkersButton.className = 'view-workers-button';
        viewWorkersButton.addEventListener('click', () => {
            fetch('http://localhost:8080/workers', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => response.json())
            .then(data => {
                // Скрываем все текущие элементы интерфейса
                managerContainer.style.display = 'none';
                sectionsContainer.style.display = 'none';
    
                // Создаем контейнер для таблицы с рабочими
                const workerTableContainer = document.createElement('div');
                workerTableContainer.className = 'worker-table-container';
    
                // Создаем заголовок "Рабочие"
                const workersTitle = document.createElement('h1');
                workersTitle.textContent = 'Рабочие';
    
                // Создаем таблицу
                const workerTable = document.createElement('table');
                workerTable.className = 'worker-table';
    
                // Создаем заголовок таблицы
                const tableHeader = document.createElement('tr');
                const nameHeader = document.createElement('th');
                nameHeader.textContent = 'Имя';
                const positionHeader = document.createElement('th');
                positionHeader.textContent = 'Должность';
                const roleHeader = document.createElement('th');
                roleHeader.textContent = 'Роль';
                const datebirthHeader = document.createElement('th');
                datebirthHeader.textContent = 'Дата рождения';
                const hiredateHeader = document.createElement('th');
                hiredateHeader.textContent = 'Дата принятия на работу';
    
                tableHeader.appendChild(nameHeader);
                tableHeader.appendChild(positionHeader);
                tableHeader.appendChild(roleHeader);
                tableHeader.appendChild(datebirthHeader);
                tableHeader.appendChild(hiredateHeader);
                workerTable.appendChild(tableHeader);
    
                // Добавляем строки с данными рабочих
                data.forEach(worker => {
                    const tableRow = document.createElement('tr');
                    const nameCell = document.createElement('td');
                    nameCell.textContent = worker.fio;
                    const positionCell = document.createElement('td');
                    positionCell.textContent = worker.position;
                    const roleCell = document.createElement('td');
                    roleCell.textContent = worker.role;
                    const dateofbirthCell = document.createElement('td');
                    dateofbirthCell.textContent = worker.dateofbirth;
                    const hiredateCell = document.createElement('td');
                    hiredateCell.textContent = worker.hiredate;
    
                    tableRow.appendChild(nameCell);
                    tableRow.appendChild(positionCell);
                    tableRow.appendChild(roleCell);
                    tableRow.appendChild(dateofbirthCell);
                    tableRow.appendChild(hiredateCell);
                    workerTable.appendChild(tableRow);
                });
    
                // Создаем кнопку "Проверено"
                const backButton = document.createElement('button');
                backButton.textContent = 'Проверено';
                backButton.className = 'back-button';
                backButton.addEventListener('click', () => {
                    workerTableContainer.remove();
                    managerContainer.style.display = 'flex';
                    sectionsContainer.style.display = 'flex';
                });
    
                // Добавляем таблицу, заголовок и кнопку в контейнер
                workerTableContainer.appendChild(workersTitle);
                workerTableContainer.appendChild(workerTable);
                workerTableContainer.appendChild(backButton);
    
                // Добавляем контейнер с таблицей в body
                document.body.appendChild(workerTableContainer);
                workerTableContainer.style.display = 'flex';
                workerTableContainer.style.flexDirection = 'column';
                workerTableContainer.style.alignItems = 'center';
            })
            .catch(error => console.error('Ошибка:', error));
        });
    
        // Добавляем заголовки и неактивные прямоугольники в соответствующие контейнеры
        materialsContainer.appendChild(materialsHeader);
        materialsContainer.appendChild(viewMaterialsButton);
        suppliesContainer.appendChild(suppliesHeader);
        suppliesContainer.appendChild(viewDeliveriesButton);
        objectsContainer.appendChild(objectsHeader);
        workersContainer.appendChild(workersHeader);
        workersContainer.appendChild(viewWorkersButton);
    
        // Запрос объектов
        fetch('http://localhost:8080/objects', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email })
        })
        .then(response => response.json())
        .then(data => {
            data.forEach(item => {
                const objectContainer = document.createElement('div');
                objectContainer.className = 'object-container';
    
                const objectName = document.createElement('button');
                objectName.textContent = item.name;
    
                const objectStatus = document.createElement('p');
                objectStatus.textContent = item.status;
    
                const objectAddress = document.createElement('p');
                objectAddress.textContent = item.address;
    
                objectContainer.appendChild(objectName);
                objectContainer.appendChild(objectAddress);
                objectContainer.appendChild(objectStatus);
    
                objectsContainer.appendChild(objectContainer);
            });
        })
        .catch(error => console.error('Ошибка:', error));
    
        // Добавляем контейнеры секций в общий контейнер
        sectionsContainer.appendChild(materialsContainer);
        sectionsContainer.appendChild(suppliesContainer);
        sectionsContainer.appendChild(objectsContainer);
        sectionsContainer.appendChild(workersContainer);
    
        // Добавляем элементы в контейнер работника
        managerContainer.appendChild(managerIcon);
        managerContainer.appendChild(managerInfo);
        managerContainer.appendChild(logoutButton);
        managerContainer.appendChild(sectionsContainer);
    
        // Добавляем контейнер работника в страницу
        document.body.appendChild(managerContainer);
    }    
}
