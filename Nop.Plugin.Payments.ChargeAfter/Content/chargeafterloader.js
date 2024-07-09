class ChargeAfterLoader {
  constructor() {
    const wrap = document.createElement('div');
    const elem = document.createElement('div');

    elem.classList.add('ca-loader');
    wrap.classList.add('ca-loader-wrap');
    wrap.style.display = 'none';
    wrap.append(elem);

    this.wrap = wrap;
  }

  onLoad() {
    document.querySelector('body').append(this.wrap);
  }

  activate() {
    this._getParentBody().classList.add('ca-loader-hidden');
    this.wrap.style.display = 'block';
  }

  deactivate() {
    this._getParentBody().classList.remove('ca-loader-hidden');
    this.wrap.style.display = 'none';
  }

  _getParentBody() {
    return this.wrap.closest('body');
  }
}

if (!window.caloader) {
  const caloader = new ChargeAfterLoader();
  caloader.onLoad();

  window.caloader = caloader;
}