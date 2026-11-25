'use strict';
const byId = id => document.getElementById(id);
const messages = {
  en: {
    filter:'Filter by product, SKU or warehouse',demo:'Live sample inventory',docs:'API reference',language:'Language',logout:'Sign out',operations:'Operations',heading:'Warehouse operations',
    intro:'Track stock, spot shortages and review the latest inventory movements.',login:'Sign in',username:'Username or email',
    password:'Password',open:'Open workspace',overview:'Inventory overview',refresh:'Refresh',alerts:'Low-stock alerts',product:'Product',
    warehouse:'Warehouse',available:'Available',threshold:'Reorder level',recent:'Recent stock movements',date:'Date',type:'Movement',
    quantity:'Quantity',products:'Products',shortages:'Low-stock items',purchases:'Pending purchases',sales:'Active sales orders',
    empty:'No records to display.',loading:'Loading inventory…',failed:'Unable to load inventory. Try refreshing.',
    denied:'Sign-in failed. Check your credentials and try again.',offline:'Could not connect to the warehouse service.',
    expired:'Your session expired. Sign in again.',moves:['','Inbound','Outbound','Transfer','Adjustment']
  },
  uk: {
    filter:'Пошук за товаром, SKU або складом',demo:'Демонстраційні складські запаси',docs:'Довідка API',language:'Мова',logout:'Вийти',operations:'Операції',heading:'Керування складом',
    intro:'Стежте за запасами, вчасно помічайте нестачу та переглядайте останні складські операції.',login:'Вхід',
    username:'Логін або електронна пошта',password:'Пароль',open:'Відкрити робоче місце',overview:'Огляд запасів',refresh:'Оновити',
    alerts:'Запаси, що потребують поповнення',product:'Товар',warehouse:'Склад',available:'Доступно',threshold:'Поріг поповнення',
    recent:'Останні переміщення',date:'Дата',type:'Операція',quantity:'Кількість',products:'Товари',shortages:'Позиції з нестачею',
    purchases:'Закупівлі в очікуванні',sales:'Активні замовлення',empty:'Записів поки немає.',loading:'Завантаження запасів…',
    failed:'Не вдалося завантажити запаси. Спробуйте оновити.',denied:'Не вдалося увійти. Перевірте логін і пароль.',
    offline:'Немає з’єднання зі складським сервісом.',expired:'Сеанс завершено. Увійдіть знову.',
    moves:['','Надходження','Відвантаження','Переміщення','Коригування']
  }
};
Object.assign(messages.en, catalogLabels.en);
Object.assign(messages.uk, catalogLabels.uk);
let lang = localStorage.getItem('wms.language') === 'uk' ? 'uk' : 'en';
let publicDemo = false;
let token = '', summary = null, lowStock = [], notice = '', error = '';
const t = key => messages[lang][key];
function fillRows(id, rows, columns) {
  const body = byId(id);
  body.replaceChildren();
  if (!rows.length) {
    const cell = body.insertRow().insertCell();
    cell.colSpan = columns;
    cell.textContent = t('empty');
  }
  for (const values of rows) {
    const row = body.insertRow();
    for (const value of values) row.insertCell().textContent = value ?? '—';
  }
}
function render() {
  document.documentElement.lang = lang;
  document.title = t('heading');
  byId('language').value = lang;
  document.querySelectorAll('[data-label]').forEach(el => el.textContent = t(el.dataset.label));
  byId('welcome').hidden = publicDemo || Boolean(token);
  byId('dashboard').hidden = !publicDemo && !token;
  byId('logout').hidden = publicDemo || !token;
  document.querySelector('a[href="/swagger"]').hidden = publicDemo;
  byId('error').textContent = error ? t(error) : '';
  byId('status').textContent = notice ? t(notice) : publicDemo ? t('demo') : '';
  renderCatalog();
  const metrics = byId('metrics');
  metrics.replaceChildren();
  if (!summary) {
    byId('alerts').replaceChildren();
    byId('movements').replaceChildren();
    return;
  }
  const number = new Intl.NumberFormat(lang);
  const date = new Intl.DateTimeFormat(lang,{dateStyle:'medium',timeStyle:'short'});
  for (const [key, value] of [['products',summary.totalProducts],['shortages',summary.lowStockItemsCount],
    ['purchases',summary.pendingPurchaseOrdersCount],['sales',summary.activeSalesOrdersCount]]) {
    const card = document.createElement('div'), amount = document.createElement('strong');
    card.className = 'metric';
    amount.textContent = number.format(value);
    card.append(document.createTextNode(t(key)), amount);
    metrics.append(card);
  }
  const query = byId('stockFilter').value.trim().toLocaleLowerCase();
  const matches = lowStock.filter(x => [x.productSku,x.productName,x.warehouseName].some(value => value.toLocaleLowerCase().includes(query)));
  fillRows('alerts',matches.map(x=>[x.productSku,x.productName,x.warehouseName,number.format(x.quantityAvailable),number.format(x.reorderLevel)]),5);
  fillRows('movements',summary.recentMovements.map(x=>[date.format(new Date(x.createdAtUtc)),x.productName,
    t('moves')[x.movementType] ?? x.movementType,number.format(x.quantity)]),4);
}
function signOut(reason = '') {
  catalog = []; planned = {};
  token = ''; summary = null; lowStock = []; notice = ''; error = reason;
  byId('login').reset(); render();
}
async function refresh() {
  const session = token;
  notice = 'loading'; render(); byId('refresh').disabled = true;
  try {
    const responses = await Promise.all(['summary','low-stock-alerts','catalog'].map(path=>
      fetch((publicDemo ? '/demo/' : '/api/dashboard/')+path,{headers:publicDemo ? {} : {Authorization:'Bearer '+session}})));
    if (session !== token) return;
    if (responses.some(r=>r.status===401)) { signOut('expired'); return; }
    if (responses.some(r=>!r.ok)) throw Error();
    const [stats,alerts,inventory] = await Promise.all(responses.map(r=>r.json()));
    if (session !== token) return;
    catalog = inventory.data; summary = stats.data; lowStock = alerts.data; notice = '';
  } catch { if (session === token) notice = 'failed'; }
  finally { byId('refresh').disabled = false; render(); }
}
byId('login').addEventListener('submit',async event=>{
  event.preventDefault();
  const button = event.currentTarget.querySelector('button');
  button.disabled = true; error = ''; render();
  try {
    const response = await fetch('/api/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},
      body:JSON.stringify({usernameOrEmail:byId('username').value.trim(),password:byId('password').value})});
    if (!response.ok) { error = 'denied'; return; }
    const payload = await response.json();
    if (!payload.data?.accessToken) { error = 'denied'; return; }
    token = payload.data.accessToken; loadPlan('account.'+byId('username').value.trim()); byId('password').value = ''; await refresh();
  } catch { error = 'offline'; }
  finally { button.disabled = false; render(); }
});
byId('logout').onclick = () => signOut();
byId('refresh').onclick = refresh;
byId('language').onchange = e => {lang=e.target.value;localStorage.setItem('wms.language',lang);render();};
render();

byId('stockFilter').addEventListener('input', render);
async function initializeWorkspace() {
  const response = await fetch('/workspace-mode');
  if (!response.ok) throw Error();
  publicDemo = (await response.json()).publicDemo;
  if (publicDemo) { loadPlan('demo'); await refresh(); }
}
initializeWorkspace().catch(() => { error = 'offline'; render(); });
