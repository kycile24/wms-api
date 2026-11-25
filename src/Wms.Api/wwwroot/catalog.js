'use strict';
const catalogLabels = {
  en:{catalog:'Stock catalog',search:'Product or SKU',category:'Category',order:'Sort by',fewest:'Lowest stock',priciest:'Highest price',bin:'Storage bin',price:'Unit price · UAH',plan:'Replenishment plan',planInfo:'Build a draft for your next purchase. Quantities are saved in this browser.',remove:'Remove',export:'Download plan as CSV',prev:'Previous',next:'Next',all:'All',add:'Add to plan',added:'In plan',found:'Stock positions',page:'Page'},
  uk:{catalog:'Каталог запасів',search:'Товар або SKU',category:'Категорія',order:'Сортування',fewest:'Найменший залишок',priciest:'Найвища ціна',bin:'Комірка',price:'Ціна за одиницю · грн',plan:'План поповнення',planInfo:'Складіть чернетку закупівлі. Кількість зберігається у цьому браузері.',remove:'Прибрати',export:'Завантажити план у CSV',prev:'Назад',next:'Далі',all:'Усі',add:'До плану',added:'У плані',found:'Складських позицій',page:'Сторінка'}
};
let catalog = [], planned = {}, planKey = '', catalogPage = 1;
const keyFor = row => JSON.stringify([row.code,row.sku]);
function loadPlan(scope) {
  planKey = 'warehouse.replenishment.'+scope;
  try { planned = JSON.parse(localStorage.getItem(planKey)) || {}; } catch { planned = {}; }
  catalogPage = 1;
}
function savePlan() { localStorage.setItem(planKey,JSON.stringify(planned)); }
function selectOptions(id,values) {
  const select = byId(id), current = select.value;
  select.replaceChildren(new Option(t('all'),''));
  for (const value of [...new Set(values)].sort()) select.add(new Option(value,value));
  select.value = values.includes(current) ? current : '';
}
function actionButton(cell,key,handler) {
  const button = document.createElement('button');
  button.type = 'button'; button.textContent = t(key); button.onclick = handler;
  cell.append(button); return button;
}
function renderCatalog() {
  selectOptions('site',catalog.map(row=>row.warehouse));
  selectOptions('category',catalog.map(row=>row.category));
  const query = byId('catalogQuery').value.trim().toLocaleLowerCase(), sort = byId('sort').value;
  const matches = catalog.filter(row => (!byId('site').value || row.warehouse === byId('site').value)
    && (!byId('category').value || row.category === byId('category').value)
    && (row.name+' '+row.sku).toLocaleLowerCase().includes(query));
  matches.sort((a,b)=> (sort === 'price' ? b.price-a.price : sort === 'available' ? a.available-b.available : 0)
    || a.name.localeCompare(b.name,lang) || a.code.localeCompare(b.code));
  const pages = Math.max(1,Math.ceil(matches.length/8));
  catalogPage = Math.min(catalogPage,pages);
  byId('found').textContent = t('found')+' · '+matches.length;
  byId('pageNumber').textContent = t('page')+' '+catalogPage+' / '+pages;
  byId('prev').disabled = catalogPage === 1; byId('next').disabled = catalogPage === pages;
  const visible = matches.slice((catalogPage-1)*8,catalogPage*8), body = byId('catalogRows');
  body.replaceChildren();
  if (!visible.length) fillRows('catalogRows',[],6);
  for(const item of visible) {
    const row = body.insertRow(), name = row.insertCell(), sku = document.createElement('small');
    name.textContent = item.name; sku.textContent = item.sku; name.append(sku);
    for(const value of [item.warehouse,item.bin]) row.insertCell().textContent = value;
    const stock = row.insertCell(); stock.textContent = item.available;
    if(item.available <= item.threshold) stock.className = 'stock-low';
    row.insertCell().textContent = item.price > 0 ? new Intl.NumberFormat(lang).format(item.price) : '—';
    const key = keyFor(item);
    actionButton(row.insertCell(),planned[key] ? 'added' : 'add',()=>{
      planned[key] = Math.max(1,item.threshold*2-item.available); savePlan(); renderCatalog();
    }).disabled = Boolean(planned[key]);
  }
  const lines = catalog.filter(item=>planned[keyFor(item)]);
  const plan = byId('planRows');plan.replaceChildren();
  if(!lines.length) fillRows('planRows',[],4);
  for(const item of lines) {
    const row = plan.insertRow(), key = keyFor(item);
    row.insertCell().textContent = item.name; row.insertCell().textContent = item.warehouse;
    const amount = document.createElement('input');amount.type='number';amount.min='1';amount.max='1000000';
    amount.value = planned[key];amount.setAttribute('aria-label',t('quantity')+' · '+item.name);
    amount.onchange = ()=>{if(amount.value && amount.checkValidity()) {planned[key]=Number(amount.value);savePlan();} else amount.value=planned[key];};
    row.insertCell().append(amount);
    actionButton(row.insertCell(),'remove',()=>{delete planned[key];savePlan();renderCatalog();});
  }
  byId('export').disabled = !lines.length;
}
for(const id of ['catalogQuery','site','category','sort']) document.getElementById(id).addEventListener('input',()=>{catalogPage=1;renderCatalog();});
document.getElementById('prev').onclick = ()=>{catalogPage--;renderCatalog();};
document.getElementById('next').onclick = ()=>{catalogPage++;renderCatalog();};
document.getElementById('export').onclick = ()=>{
  const rows = [['SKU',t('product'),t('warehouse'),t('quantity')]];
  for(const item of catalog) if(planned[keyFor(item)]) rows.push([item.sku,item.name,item.warehouse,planned[keyFor(item)]]);
  const escape = value => '"'+String(value).replace(/^[=+@\-\t\r]/,"'$&").replaceAll('"','""')+'"';
  const csv = rows.map(row=>row.map(escape).join(',')).join('\r\n');
  const url=URL.createObjectURL(new Blob(['\uFEFF'+csv],{type:'text/csv;charset=utf-8'}));
  const link=document.createElement('a');link.href=url;link.download='replenishment.csv';link.click();setTimeout(()=>URL.revokeObjectURL(url),1000);
};
